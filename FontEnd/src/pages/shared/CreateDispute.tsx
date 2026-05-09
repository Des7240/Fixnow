import { useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ChevronLeft, UploadCloud, AlertTriangle } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';

export default function CreateDispute() {
  const { id } = useParams(); // BookingId
  const navigate = useNavigate();
  const [reason, setReason] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0];
    if (selected) {
      setFile(selected);
      const url = URL.createObjectURL(selected);
      setPreview(url);
    }
  };

  const handleSubmit = async () => {
    if (!reason.trim()) {
      message.error('Vui lòng nhập lý do khiếu nại');
      return;
    }

    setLoading(true);
    try {
      // 1. Create Dispute
      const disputeRes = await axiosInstance.post('/disputes', {
        bookingId: id,
        reason: reason
      });

      const disputeId = disputeRes.data.id;

      // 2. Upload Evidence if any
      if (file) {
        const formData = new FormData();
        formData.append('file', file);
        await axiosInstance.post(`/disputes/${disputeId}/evidences`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
      }

      message.success('Đã gửi khiếu nại. Admin sẽ sớm xử lý cho bạn.');
      navigate(`/customer/bookings/${id}`);
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Có lỗi xảy ra khi tạo khiếu nại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <div className="bg-white px-4 py-4 shadow-sm z-20 flex items-center gap-4">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 hover:bg-gray-100 rounded-full transition-all">
          <ChevronLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-lg font-bold text-gray-900">Khiếu nại Dịch vụ</h1>
      </div>

      <div className="flex-1 p-4 max-w-lg mx-auto w-full">
        <div className="bg-red-50 p-4 rounded-2xl flex items-start gap-3 mb-6">
          <AlertTriangle className="w-6 h-6 text-red-500 flex-shrink-0 mt-0.5" />
          <p className="text-sm text-red-800">
            Nếu bạn không hài lòng về chất lượng sửa chữa hoặc có vấn đề gian lận, hãy mô tả chi tiết để Admin can thiệp và hỗ trợ hoàn tiền.
          </p>
        </div>

        <div className="space-y-6">
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <label className="block text-sm font-bold text-gray-900 mb-3">Lý do khiếu nại</label>
            <textarea
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="Mô tả cụ thể vấn đề bạn gặp phải..."
              rows={5}
              className="w-full bg-gray-50 border-none rounded-2xl p-4 text-sm focus:ring-2 focus:ring-red-500/50 resize-none"
            />
          </div>

          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <label className="block text-sm font-bold text-gray-900 mb-3">Hình ảnh chứng minh (Tuỳ chọn)</label>
            <div 
              onClick={() => fileInputRef.current?.click()}
              className="w-full border-2 border-dashed border-gray-300 rounded-2xl p-8 flex flex-col items-center justify-center text-center cursor-pointer hover:bg-gray-50 transition-colors"
            >
              {preview ? (
                <img src={preview} alt="Preview" className="max-h-40 rounded-xl mb-3" />
              ) : (
                <div className="w-12 h-12 bg-red-50 text-red-500 rounded-full flex items-center justify-center mb-3">
                  <UploadCloud className="w-6 h-6" />
                </div>
              )}
              <span className="text-sm font-bold text-gray-600">
                {preview ? 'Đổi ảnh khác' : 'Nhấn để tải ảnh lên'}
              </span>
            </div>
            <input 
              type="file" 
              ref={fileInputRef} 
              onChange={handleFileChange} 
              accept="image/*" 
              className="hidden" 
            />
          </div>
        </div>
      </div>

      <div className="p-4 bg-white/80 backdrop-blur-lg border-t border-gray-100 pb-safe">
        <button
          onClick={handleSubmit}
          disabled={loading}
          className="w-full max-w-lg mx-auto block py-4 bg-red-500 hover:bg-red-600 text-white font-bold rounded-2xl shadow-xl shadow-red-500/20 disabled:opacity-50 transition-all flex justify-center items-center"
        >
          {loading ? <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span> : 'Gửi Khiếu Nại'}
        </button>
      </div>
    </div>
  );
}
