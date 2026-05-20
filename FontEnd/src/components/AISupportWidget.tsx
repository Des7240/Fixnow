import React, { useState, useRef } from 'react';
import { MessageCircle, X, Image as ImageIcon, Send, Sparkles } from 'lucide-react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { aiSupportApi } from '../api/aiSupportApi';
import { message } from 'antd';
import clsx from 'clsx';

export const AISupportWidget: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [problemDescription, setProblemDescription] = useState('');
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [response, setResponse] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Dragging state
  const [position, setPosition] = useState<{ x: number, y: number } | null>(null);
  const startMousePos = useRef({ x: 0, y: 0 });
  const startElementPos = useRef({ x: 0, y: 0 });
  const isMoved = useRef(false);

  const handlePointerDown = (e: React.PointerEvent<HTMLButtonElement>) => {
    if (e.button !== 0) return;
    
    // We attach dragging to the wrapper div to move everything together
    const target = e.currentTarget.parentElement as HTMLDivElement;
    const rect = target.getBoundingClientRect();
    
    startElementPos.current = position ? position : { x: rect.left, y: rect.top };
    startMousePos.current = { x: e.clientX, y: e.clientY };
    isMoved.current = false;

    const handleMove = (moveEvent: PointerEvent) => {
      const dx = moveEvent.clientX - startMousePos.current.x;
      const dy = moveEvent.clientY - startMousePos.current.y;
      
      if (Math.abs(dx) > 3 || Math.abs(dy) > 3) {
        isMoved.current = true;
      }
      
      setPosition({
        x: startElementPos.current.x + dx,
        y: startElementPos.current.y + dy,
      });
    };

    const handleUp = () => {
      window.removeEventListener('pointermove', handleMove);
      window.removeEventListener('pointerup', handleUp);
    };

    window.addEventListener('pointermove', handleMove);
    window.addEventListener('pointerup', handleUp);
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setImageFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleRemoveImage = () => {
    setImageFile(null);
    setImagePreview(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!problemDescription.trim() && !imageFile) {
      message.warning('Vui lòng nhập mô tả hoặc tải ảnh lên để AI phân tích.');
      return;
    }

    setIsLoading(true);
    setResponse(null);
    try {
      const res = await aiSupportApi.analyzeProblem(problemDescription, imageFile);
      setResponse(res.responseText);
    } catch (error: any) {
      message.error(error?.response?.data?.message || 'Có lỗi xảy ra khi kết nối với AI.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div 
      className={clsx("fixed z-[100]", !position && "bottom-24 lg:bottom-6 right-6")}
      style={position ? { left: position.x, top: position.y } : {}}
    >
      {/* Widget Button */}
      <button
        onPointerDown={handlePointerDown}
        onClick={(e) => {
          if (isMoved.current) {
            e.preventDefault();
            e.stopPropagation();
            isMoved.current = false;
            return;
          }
          setIsOpen(!isOpen);
        }}
        className={clsx(
          "flex items-center justify-center w-14 h-14 rounded-full shadow-lg transition-all duration-300",
          isOpen ? "bg-gray-800 text-white rotate-90" : "bg-gradient-to-r from-orange-500 to-amber-500 text-white hover:scale-105 hover:shadow-xl"
        )}
      >
        {isOpen ? <X size={28} className="-rotate-90" /> : <Sparkles size={28} />}
      </button>

      {/* Chat Window */}
      {isOpen && (
        <div className="absolute bottom-20 right-0 w-80 sm:w-96 bg-white/95 backdrop-blur-md border border-gray-200 rounded-2xl shadow-2xl overflow-hidden flex flex-col transition-all duration-300 transform origin-bottom-right">
          {/* Header */}
          <div className="bg-gradient-to-r from-orange-500 to-amber-500 p-4 text-white flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <Sparkles size={20} />
              <h3 className="font-semibold text-lg">Trợ lý AI FixNow</h3>
            </div>
            <button onClick={() => setIsOpen(false)} className="text-white/80 hover:text-white">
              <X size={20} />
            </button>
          </div>

          {/* Body */}
          <div className="flex-1 p-4 overflow-y-auto min-h-[300px] max-h-[400px] bg-gray-50/50">
            {!response && !isLoading && (
              <div className="text-center text-gray-500 mt-10">
                <MessageCircle size={40} className="mx-auto mb-3 text-gray-300" />
                <p className="text-sm">Hãy mô tả sự cố hoặc tải ảnh lên để AI giúp bạn chẩn đoán vấn đề nhé!</p>
              </div>
            )}

            {isLoading && (
              <div className="flex items-center justify-center h-full mt-20">
                <div className="animate-pulse flex flex-col items-center">
                  <div className="h-10 w-10 bg-orange-200 rounded-full mb-3 flex items-center justify-center">
                    <Sparkles className="text-orange-500 animate-spin" size={20} />
                  </div>
                  <p className="text-sm text-gray-500">AI đang phân tích...</p>
                </div>
              </div>
            )}

            {response && !isLoading && (
              <div className="bg-white p-4 rounded-xl shadow-sm border border-orange-100 prose prose-sm prose-orange max-w-none prose-headings:text-gray-800 prose-p:text-gray-600 prose-a:text-orange-500">
                <ReactMarkdown remarkPlugins={[remarkGfm]}>
                  {response}
                </ReactMarkdown>
              </div>
            )}
          </div>

          {/* Input Area */}
          <div className="p-3 border-t border-gray-100 bg-white">
            {imagePreview && (
              <div className="mb-2 relative inline-block">
                <img src={imagePreview} alt="Preview" className="h-16 w-16 object-cover rounded-lg border border-gray-200" />
                <button
                  onClick={handleRemoveImage}
                  className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-0.5 hover:bg-red-600 shadow-sm"
                >
                  <X size={12} />
                </button>
              </div>
            )}
            
            <form onSubmit={handleSubmit} className="flex items-end space-x-2">
              <div className="flex-1 bg-gray-100 rounded-xl border border-transparent focus-within:border-orange-300 focus-within:bg-white transition-all overflow-hidden flex items-end">
                <textarea
                  value={problemDescription}
                  onChange={(e) => setProblemDescription(e.target.value)}
                  placeholder="Mô tả lỗi..."
                  className="w-full max-h-32 min-h-[44px] py-3 px-3 bg-transparent border-none focus:ring-0 resize-none text-sm"
                  rows={problemDescription.split('\n').length > 1 ? Math.min(3, problemDescription.split('\n').length) : 1}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && !e.shiftKey) {
                      e.preventDefault();
                      handleSubmit(e);
                    }
                  }}
                />
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  className="p-3 text-gray-400 hover:text-orange-500 transition-colors"
                  title="Đính kèm ảnh"
                >
                  <ImageIcon size={20} />
                </button>
                <input
                  type="file"
                  ref={fileInputRef}
                  onChange={handleImageChange}
                  accept="image/*"
                  className="hidden"
                />
              </div>
              <button
                type="submit"
                disabled={isLoading || (!problemDescription.trim() && !imageFile)}
                className="bg-orange-500 text-white p-3 rounded-xl hover:bg-orange-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-md shadow-orange-500/20"
              >
                <Send size={20} />
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
