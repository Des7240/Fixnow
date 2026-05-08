import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowLeft, MapPin, Wrench, Clock, FileText } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';

const bookingSchema = z.object({
  serviceId: z.string().min(1, 'Vui lòng chọn dịch vụ'),
  address: z.string().min(5, 'Địa chỉ chi tiết không được để trống'),
  lat: z.number(),
  lng: z.number(),
  notes: z.string().optional(),
});

type BookingForm = z.infer<typeof bookingSchema>;

// Mock services
const SERVICES = [
  { id: '1', name: 'Sửa điện', icon: '⚡' },
  { id: '2', name: 'Sửa nước', icon: '💧' },
  { id: '3', name: 'Sửa điều hoà', icon: '❄️' },
  { id: '4', name: 'Sửa khoá', icon: '🔑' },
];

export default function CreateBooking() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  // In real app, lat lng comes from Geolocation/Map Picker
  const { register, handleSubmit, setValue, watch, formState: { errors } } = useForm<BookingForm>({
    resolver: zodResolver(bookingSchema),
    defaultValues: {
      lat: 21.0285,
      lng: 105.8048
    }
  });

  const selectedService = watch('serviceId');

  const onSubmit = async (data: BookingForm) => {
    setLoading(true);
    try {
      // Assuming your backend uses exact UUIDs, you'll need real service IDs here
      // For MVP demo, if backend throws 400 because '1' is not UUID, we handle it
      await axiosInstance.post('/bookings', data);
      message.success('Đặt thợ thành công! Đang tìm thợ gần bạn...');
      navigate('/customer/bookings');
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      message.error(error?.response?.data?.message || 'Có lỗi xảy ra khi đặt thợ');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white px-4 py-4 flex items-center gap-4 shadow-sm z-10">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 rounded-full hover:bg-gray-100">
          <ArrowLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-lg font-bold text-gray-900">Chi tiết đặt thợ</h1>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 max-w-md mx-auto pb-24">
          
          {/* Service Selection */}
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
              <Wrench className="w-5 h-5 text-orange-500" />
              <h2>Chọn dịch vụ</h2>
            </div>
            <div className="grid grid-cols-2 gap-3">
              {SERVICES.map((srv) => (
                <div 
                  key={srv.id}
                  onClick={() => setValue('serviceId', srv.id, { shouldValidate: true })}
                  className={`flex items-center gap-3 p-3 rounded-2xl border-2 transition-all cursor-pointer ${
                    selectedService === srv.id 
                      ? 'border-orange-500 bg-orange-50' 
                      : 'border-gray-100 bg-gray-50 hover:border-gray-200'
                  }`}
                >
                  <span className="text-xl">{srv.icon}</span>
                  <span className="text-sm font-semibold text-gray-700">{srv.name}</span>
                </div>
              ))}
            </div>
            {errors.serviceId && <p className="text-red-500 text-xs mt-2 ml-1">{errors.serviceId.message}</p>}
          </div>

          {/* Location details */}
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
              <MapPin className="w-5 h-5 text-orange-500" />
              <h2>Địa chỉ sửa chữa</h2>
            </div>
            <textarea
              {...register('address')}
              placeholder="Nhập số nhà, ngõ, tên đường..."
              rows={3}
              className="w-full bg-gray-50 rounded-2xl border-none px-4 py-3 text-sm text-gray-900 focus:ring-2 focus:ring-orange-500/50 resize-none"
            />
            {errors.address && <p className="text-red-500 text-xs mt-2 ml-1">{errors.address.message}</p>}
            
            <div className="mt-3 flex items-center justify-between p-3 bg-blue-50 text-blue-700 rounded-xl text-xs font-medium">
              <span>Định vị GPS đã được bật tự động</span>
              <div className="w-2 h-2 rounded-full bg-blue-500 animate-pulse"></div>
            </div>
          </div>

          {/* Notes */}
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
              <FileText className="w-5 h-5 text-orange-500" />
              <h2>Mô tả tình trạng (Tuỳ chọn)</h2>
            </div>
            <textarea
              {...register('notes')}
              placeholder="Ví dụ: Nước rò rỉ mạnh ở bồn rửa mặt..."
              rows={3}
              className="w-full bg-gray-50 rounded-2xl border-none px-4 py-3 text-sm text-gray-900 focus:ring-2 focus:ring-orange-500/50 resize-none"
            />
          </div>

          {/* Timing info */}
          <div className="flex items-center gap-3 p-4 bg-orange-50 rounded-2xl text-orange-800">
            <Clock className="w-6 h-6 text-orange-500 flex-shrink-0" />
            <p className="text-sm font-medium">Thợ sẽ liên hệ và báo giá trực tiếp trong vòng 15 phút sau khi nhận đơn.</p>
          </div>
        </form>
      </div>

      {/* Floating Action Button */}
      <div className="fixed bottom-0 left-0 right-0 p-4 bg-gradient-to-t from-white via-white to-transparent">
        <button
          onClick={handleSubmit(onSubmit)}
          disabled={loading}
          className="w-full max-w-md mx-auto block py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl shadow-xl shadow-gray-900/20 transition-all disabled:opacity-70 flex items-center justify-center gap-2"
        >
          {loading && <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />}
          {loading ? 'Đang tìm thợ...' : 'Tìm thợ ngay'}
        </button>
      </div>
    </div>
  );
}
