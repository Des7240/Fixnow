import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowLeft, MapPin, Wrench, Clock, FileText, Navigation, Loader2, Home, Camera, X } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { getImageUrl } from '../../utils/constants';

const bookingSchema = z.object({
  serviceId: z.string().min(1, 'Vui lòng chọn dịch vụ'),
  address: z.string().min(5, 'Địa chỉ chi tiết không được để trống'),
  detailAddress: z.string().min(1, 'Vui lòng nhập số nhà, tầng, phòng...'),
  lat: z.number(),
  lng: z.number(),
  description: z.string().optional(),
  fileUrls: z.array(z.string()).optional()
});

type BookingForm = z.infer<typeof bookingSchema>;

interface Service {
  id: string;
  name: string;
  iconUrl?: string;
}

export default function CreateBooking() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialServiceId = searchParams.get('serviceId');
  
  const [loading, setLoading] = useState(false);
  const [services, setServices] = useState<Service[]>([]);
  const [locating, setLocating] = useState(false);
  const [uploadingImages, setUploadingImages] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { register, handleSubmit, setValue, watch, formState: { errors } } = useForm<BookingForm>({
    resolver: zodResolver(bookingSchema),
    defaultValues: {
      lat: 21.0285,
      lng: 105.8048,
      serviceId: initialServiceId || '',
      address: '',
      detailAddress: '',
      fileUrls: []
    }
  });

  useEffect(() => {
    // Fetch real services from database
    const fetchServices = async () => {
      try {
        const res = await axiosInstance.get('/services');
        setServices(res.data);
        
        // If initialServiceId is provided, set it
        if (initialServiceId) {
          setValue('serviceId', initialServiceId, { shouldValidate: true });
        }
      } catch (err) {
        console.error('Failed to fetch services', err);
      }
    };
    fetchServices();
  }, [initialServiceId, setValue]);

  const handleGetCurrentLocation = () => {
    setLocating(true);
    if (!navigator.geolocation) {
      message.error('Trình duyệt không hỗ trợ lấy vị trí.');
      setLocating(false);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const lat = position.coords.latitude;
        const lng = position.coords.longitude;
        setValue('lat', lat);
        setValue('lng', lng);
        
        // Reverse geocoding optional API
        try {
          const res = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`);
          const data = await res.json();
          if (data.display_name) {
            setValue('address', data.display_name);
          }
        } catch {
          setValue('address', `${lat}, ${lng}`); // Fallback
        }
        
        message.success('Đã lấy vị trí hiện tại');
        setLocating(false);
      },
      (error) => {
        console.error(error);
        message.error('Không thể lấy vị trí. Hãy chắc chắn bạn đã cấp quyền cho trình duyệt.');
        setLocating(false);
      },
      { timeout: 10000, enableHighAccuracy: true }
    );
  };

  const selectedService = watch('serviceId');
  const fileUrls = watch('fileUrls') || [];

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    if (fileUrls.length + files.length > 5) {
      message.warning('Chỉ được tải lên tối đa 5 ảnh');
      return;
    }

    setUploadingImages(true);
    try {
      const uploadPromises = files.map(file => {
        const formData = new FormData();
        formData.append('file', file);
        return axiosInstance.post('/files/upload', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
      });

      const responses = await Promise.all(uploadPromises);
      const newUrls = responses.map(res => res.data.objectKey);
      
      setValue('fileUrls', [...fileUrls, ...newUrls], { shouldValidate: true });
      message.success('Tải ảnh lên thành công');
    } catch (err) {
      message.error('Lỗi khi tải ảnh lên');
      console.error(err);
    } finally {
      setUploadingImages(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const removeImage = (index: number) => {
    const newUrls = [...fileUrls];
    newUrls.splice(index, 1);
    setValue('fileUrls', newUrls, { shouldValidate: true });
  };

  const onSubmit = async (data: BookingForm) => {
    setLoading(true);
    try {
      const fullAddress = `${data.detailAddress}, ${data.address}`;
      await axiosInstance.post('/bookings', {
        ...data,
        address: fullAddress
      });
      message.success('Đặt thợ thành công! Đang tìm thợ gần bạn...');
      navigate('/customer/bookings');
    } catch (err: any) {
      const errorMsg = err?.response?.data?.message || 'Có lỗi xảy ra khi đặt thợ';
      message.error(errorMsg);
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
              {services.map((srv) => (
                <div 
                  key={srv.id}
                  onClick={() => setValue('serviceId', srv.id, { shouldValidate: true })}
                  className={`flex items-center gap-3 p-3 rounded-2xl border-2 transition-all cursor-pointer ${
                    selectedService === srv.id 
                      ? 'border-orange-500 bg-orange-50' 
                      : 'border-gray-100 bg-gray-50 hover:border-gray-200'
                  }`}
                >
                  <span className="text-xl">{srv.iconUrl && srv.iconUrl.length < 5 ? srv.iconUrl : '⚡'}</span>
                  <span className="text-sm font-semibold text-gray-700">{srv.name}</span>
                </div>
              ))}
            </div>
            {errors.serviceId && <p className="text-red-500 text-xs mt-2 ml-1">{errors.serviceId.message}</p>}
          </div>

          {/* Location details */}
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2 text-gray-900 font-bold">
                <MapPin className="w-5 h-5 text-orange-500" />
                <h2>Địa chỉ sửa chữa</h2>
              </div>
              <button 
                type="button" 
                onClick={handleGetCurrentLocation}
                disabled={locating}
                className="text-xs bg-orange-100 text-orange-600 px-3 py-1.5 rounded-full font-semibold flex items-center gap-1"
              >
                {locating ? <Loader2 className="w-3 h-3 animate-spin"/> : <Navigation className="w-3 h-3" />} Lấy vị trí
              </button>
            </div>

            <div className="space-y-3">
                <div className="relative">
                    <Home className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
                    <input
                        {...register('detailAddress')}
                        placeholder="Số nhà, tầng, tên tòa nhà..."
                        className="w-full bg-gray-50 rounded-xl border-none pl-10 pr-4 py-3 text-sm text-gray-900 focus:ring-2 focus:ring-orange-500/50"
                    />
                </div>
                {errors.detailAddress && <p className="text-red-500 text-xs mt-1 ml-1">{errors.detailAddress.message}</p>}

                <textarea
                    {...register('address')}
                    placeholder="Phường/Xã, Quận/Huyện, Thành phố (Tự động hoặc nhập tay)..."
                    rows={2}
                    className="w-full bg-gray-50 rounded-xl border-none px-4 py-3 text-sm text-gray-900 focus:ring-2 focus:ring-orange-500/50 resize-none"
                />
                {errors.address && <p className="text-red-500 text-xs mt-1 ml-1">{errors.address.message}</p>}
            </div>
            
            <div className="mt-3 flex items-center justify-between p-3 bg-blue-50 text-blue-700 rounded-xl text-xs font-medium">
              <span>Định vị GPS hỗ trợ tìm thợ gần nhất</span>
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
              {...register('description')}
              placeholder="Ví dụ: Nước rò rỉ mạnh ở bồn rửa mặt..."
              rows={3}
              className="w-full bg-gray-50 rounded-2xl border-none px-4 py-3 text-sm text-gray-900 focus:ring-2 focus:ring-orange-500/50 resize-none"
            />
            
            {/* Image Upload Area */}
            <div className="mt-4">
              <div className="flex gap-2 overflow-x-auto pb-2">
                {fileUrls.map((url, idx) => (
                  <div key={idx} className="relative w-20 h-20 flex-shrink-0">
                    <img src={getImageUrl(url)} alt={`upload-${idx}`} className="w-full h-full object-cover rounded-xl border border-gray-200" />
                    <button
                      type="button"
                      onClick={() => removeImage(idx)}
                      className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-1 shadow-sm hover:bg-red-600"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </div>
                ))}
                
                {fileUrls.length < 5 && (
                  <button
                    type="button"
                    onClick={() => fileInputRef.current?.click()}
                    disabled={uploadingImages}
                    className="w-20 h-20 flex-shrink-0 flex flex-col items-center justify-center gap-1 bg-gray-50 rounded-xl border-2 border-dashed border-gray-300 hover:border-orange-500 hover:bg-orange-50 transition-colors disabled:opacity-50"
                  >
                    {uploadingImages ? (
                      <Loader2 className="w-5 h-5 text-orange-500 animate-spin" />
                    ) : (
                      <>
                        <Camera className="w-5 h-5 text-gray-400" />
                        <span className="text-[10px] text-gray-400 font-medium">{fileUrls.length}/5</span>
                      </>
                    )}
                  </button>
                )}
              </div>
              <input 
                type="file" 
                ref={fileInputRef}
                onChange={handleImageUpload}
                accept="image/*"
                multiple
                className="hidden"
              />
            </div>
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
