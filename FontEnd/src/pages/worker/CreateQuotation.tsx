import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ChevronLeft, Plus, Trash2, CheckCircle } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { useAuthStore } from '../../stores/authStore';

export default function CreateQuotation() {
  const { id } = useParams(); // Booking ID
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const [items, setItems] = useState([{ itemName: '', quantity: 1, unitPrice: 0 }]);

  const addItem = () => {
    setItems([...items, { itemName: '', quantity: 1, unitPrice: 0 }]);
  };

  const removeItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleChange = (index: number, field: string, value: any) => {
    const newItems = [...items];
    (newItems[index] as any)[field] = value;
    setItems(newItems);
  };

  const calculateTotal = () => {
    return items.reduce((sum, item) => sum + (item.quantity * item.unitPrice), 0);
  };

  const submitQuotation = async () => {
    if (items.length === 0 || items.some(i => !i.itemName || i.unitPrice <= 0)) {
      message.error('Vui lòng điền đầy đủ tên và đơn giá hợp lệ cho các hạng mục');
      return;
    }

    setLoading(true);
    try {
      await axiosInstance.post('/quotations', {
        bookingId: id,
        workerId: user?.id,
        items: items
      });
      message.success('Đã gửi báo giá cho khách hàng');
      navigate(`/worker/bookings/${id}`);
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi gửi báo giá');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 pb-32">
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm sticky top-0 z-20 flex items-center gap-4">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 hover:bg-gray-100 rounded-full transition-all">
          <ChevronLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-xl font-bold text-gray-900">Tạo báo giá</h1>
      </div>

      <div className="p-4 space-y-4 max-w-lg mx-auto">
        {items.map((item, idx) => (
          <div key={idx} className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex justify-between mb-3">
              <span className="font-bold text-gray-700">Hạng mục {idx + 1}</span>
              {items.length > 1 && (
                <button onClick={() => removeItem(idx)} className="text-red-500 hover:bg-red-50 p-1 rounded">
                  <Trash2 className="w-4 h-4" />
                </button>
              )}
            </div>
            <input
              placeholder="Tên hạng mục (vd: Thay vòi nước)"
              className="w-full mb-3 p-3 bg-gray-50 rounded-xl border-none focus:ring-2 focus:ring-orange-500/50 text-sm"
              value={item.itemName}
              onChange={(e) => handleChange(idx, 'itemName', e.target.value)}
            />
            <div className="flex gap-3">
              <div className="flex-1">
                <label className="text-xs text-gray-500 mb-1 block">Số lượng</label>
                <input
                  type="number"
                  min="1"
                  className="w-full p-3 bg-gray-50 rounded-xl border-none focus:ring-2 focus:ring-orange-500/50 text-sm"
                  value={item.quantity}
                  onChange={(e) => handleChange(idx, 'quantity', parseInt(e.target.value))}
                />
              </div>
              <div className="flex-[2]">
                <label className="text-xs text-gray-500 mb-1 block">Đơn giá (VND)</label>
                <input
                  type="number"
                  min="0"
                  className="w-full p-3 bg-gray-50 rounded-xl border-none focus:ring-2 focus:ring-orange-500/50 text-sm"
                  value={item.unitPrice}
                  onChange={(e) => handleChange(idx, 'unitPrice', parseInt(e.target.value))}
                />
              </div>
            </div>
          </div>
        ))}

        <button 
          onClick={addItem}
          className="w-full py-4 border-2 border-dashed border-gray-300 text-gray-600 rounded-2xl hover:bg-gray-100 flex items-center justify-center gap-2 font-bold transition-all"
        >
          <Plus className="w-5 h-5" /> Thêm hạng mục
        </button>
      </div>

      <div className="fixed bottom-0 left-0 right-0 p-4 bg-white/80 backdrop-blur-lg border-t border-gray-100 z-30">
        <div className="max-w-lg mx-auto flex items-center justify-between gap-4">
          <div>
            <p className="text-xs text-gray-500 font-bold uppercase">Tổng cộng</p>
            <p className="text-xl font-black text-orange-600">
              {calculateTotal().toLocaleString('vi-VN')} đ
            </p>
          </div>
          <button 
            onClick={submitQuotation}
            disabled={loading}
            className="flex-[2] py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl shadow-xl shadow-gray-900/20 transition-all flex items-center justify-center gap-2 disabled:opacity-70"
          >
            {loading ? <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span> : <CheckCircle className="w-5 h-5" />}
            Gửi báo giá
          </button>
        </div>
      </div>
    </div>
  );
}
