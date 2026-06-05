import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ChevronLeft, Send, Image as ImageIcon } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { getImageUrl } from '../../utils/constants';
import { useAuthStore } from '../../stores/authStore';
import { useChatStore } from '../../stores/chatStore';
import { useSignalR } from '../../signalr/SignalRContext';
import { clsx } from 'clsx';

export default function ChatRoom() {
  const { id } = useParams(); // bookingId
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const { messages, setMessages, addMessage } = useChatStore();
  const { connection } = useSignalR();
  const [inputText, setInputText] = useState('');
  const [loading, setLoading] = useState(true);
  const [conversation, setConversation] = useState<any>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    fetchHistory();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  useEffect(() => {
    if (connection && conversation) {
      // Join the conversation group
      connection.invoke("JoinConversation", conversation.id).catch(err => console.error(err));

      connection.on('ReceiveMessage', (msg) => {
        if (msg.conversationId === conversation.id) {
          addMessage(msg);
        }
      });

      return () => {
        connection.off('ReceiveMessage');
      };
    }
  }, [connection, conversation, addMessage]);

  const fetchHistory = async () => {
    try {
      setLoading(true);
      const res = await axiosInstance.get(`/chat/by-booking/${id}`);
      setConversation(res.data.conversation);
      setMessages(res.data.messages);
    } catch (err) {
      console.error(err);
      message.error('Không thể tải lịch sử tin nhắn');
    } finally {
      setLoading(false);
    }
  };

  const sendMessage = async () => {
    if (!inputText.trim() || !conversation) return;
    try {
      const text = inputText;
      setInputText('');
      const res = await axiosInstance.post('/chat/messages', {
        conversationId: conversation.id,
        content: text,
        messageType: 'TEXT'
      });

      // Add message immediately for realtime feedback
      addMessage(res.data);
    } catch (err) {
      message.error('Không gửi được tin nhắn');
    }
  };

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !conversation) return;

    const formData = new FormData();
    formData.append('file', file);

    try {
      const uploadRes = await axiosInstance.post('/files/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });

      const res = await axiosInstance.post('/chat/messages', {
        conversationId: conversation.id,
        messageType: 'IMAGE',
        fileIds: [uploadRes.data.id]
      });

      addMessage(res.data);
    } catch (err) {
      message.error('Không gửi được hình ảnh');
    }
  };

  if (loading) {
    return (
      <div className="h-screen flex items-center justify-center bg-gray-50">
        <span className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin"></span>
      </div>
    );
  }

  const otherName = user?.role === 'CUSTOMER' ? conversation?.workerName : conversation?.customerName;

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-white px-4 py-4 shadow-sm z-20 flex items-center gap-4">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 hover:bg-gray-100 rounded-full transition-all">
          <ChevronLeft className="w-6 h-6 text-gray-700" />
        </button>
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-orange-100 rounded-full flex items-center justify-center font-bold text-orange-600">
            {otherName?.charAt(0) || '?'}
          </div>
          <div>
            <h1 className="text-sm font-bold text-gray-900">{otherName || 'Đang tải...'}</h1>
            <p className="text-[10px] text-green-500 font-bold">● Đang hoạt động</p>
          </div>
        </div>
      </div>

      {/* Chat Area */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {messages.map((msg) => {
          const isMine = msg.senderId === user?.id;
          return (
            <div key={msg.id} className={clsx("flex flex-col max-w-[80%]", isMine ? "items-end self-end ml-auto" : "items-start")}>
              <div className={clsx(
                "px-4 py-2.5 rounded-2xl shadow-sm break-words",
                isMine ? "bg-orange-500 text-white rounded-br-none" : "bg-white text-gray-800 rounded-bl-none border border-gray-100"
              )}>
                {msg.messageType === 'IMAGE' ? (
                  <div className="space-y-1">
                    {msg.attachmentUrls?.map((url: string, idx: number) => (
                      <img key={idx} src={getImageUrl(url)} alt="Attachment" className="max-w-[200px] rounded-lg" />
                    ))}
                    {msg.content && <p className="text-sm">{msg.content}</p>}
                  </div>
                ) : (
                  <p className="text-sm">{msg.content}</p>
                )}
              </div>
              <span className="text-[10px] text-gray-400 mt-1 px-1">
                {new Date(msg.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
              </span>
            </div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>

      {/* Input Area */}
      <div className="bg-white p-4 border-t border-gray-100 pb-safe">
        <div className="flex items-end gap-2 bg-gray-50 p-2 rounded-3xl border border-gray-200 focus-within:border-orange-300 focus-within:ring-2 focus-within:ring-orange-500/20 transition-all">
          <button
            onClick={() => fileInputRef.current?.click()}
            className="p-2 text-gray-400 hover:text-orange-500 transition-colors"
          >
            <ImageIcon className="w-6 h-6" />
          </button>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleImageUpload}
            accept="image/*"
            className="hidden"
          />
          <textarea
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
            placeholder="Nhập tin nhắn..."
            rows={1}
            className="flex-1 bg-transparent border-none py-2 px-1 text-sm resize-none focus:ring-0 max-h-24"
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
              }
            }}
          />
          <button
            onClick={sendMessage}
            disabled={!inputText.trim()}
            className="p-2 text-white bg-orange-500 rounded-full hover:bg-orange-600 disabled:bg-gray-300 transition-all flex-shrink-0"
          >
            <Send className="w-5 h-5 ml-0.5" />
          </button>
        </div>
      </div>
    </div>
  );
}
