import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowLeft, MapPin, Wrench, Clock, FileText, Navigation, Loader2, Upload, X, Shield, DollarSign, Home } from 'lucide-react';
import { message, Select, InputNumber } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { API_BASE_URL } from '../../utils/constants';

const openJobSchema = z.object({
  serviceId: z.string().min(1, 'Vui lòng chọn dịch vụ'),
  title: z.string().min(5, 'Tiêu đề ít nhất 5 ký tự'),
  description: z.string().min(10, 'Mô tả chi tiết tình trạng để thợ dễ báo giá'),
  address: z.string().min(5, 'Địa chỉ không được để trống'),
  detailAddress: z.string().min(1, 'Vui lòng nhập số nhà, tầng, phòng...'),
  lat: z.number(),
  lng: z.number(),
  radiusKm: z.number().min(1).max(50),
  minBudget: z.number().min(0, 'Giá không được âm').optional(),
  maxBudget: z.number().min(0, 'Giá không được âm').optional(),
  urgencyLevel: z.string().optional(),
});

type OpenJobForm = z.infer<typeof openJobSchema>;

interface Service {
  id: string;
  name: string;
  iconUrl?: string;
}

export default function CreateOpenJob() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialServiceId = searchParams.get('serviceId');

  const [loading, setLoading] = useState(false);
  const [services, setServices] = useState<Service[]>([]);
  const [locating, setLocating] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [fileIds, setFileIds] = useState<string[]>([]);
  const [previewUrls, setPreviewUrls] = useState<string[]>([]);

  const { register, handleSubmit, setValue, watch, formState: { errors } } = useForm<OpenJobForm>({
    resolver: zodResolver(openJobSchema),
    defaultValues: {
      lat: 21.0285,
      lng: 105.8048,
      radiusKm: 5,
      serviceId: initialServiceId || '',
      address: '',
      detailAddress: ''
    }
  });

  useEffect(() => {
    const fetchServices = async () => {
      try {
        const res = await axiosInstance.get('/services');
        setServices(res.data);
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
        
        try {
          const res = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`);
          const data = await res.json();
          if (data.display_name) {
            setValue('address', data.display_name);
          }
        } catch {
          setValue('address', `${lat}, ${lng}`);
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

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    setUploading(true);
    try {
      const newFileIds = [...fileIds];
      const newPreviewUrls = [...previewUrls];

      for (let i = 0; i < files.length; i++) {
        const singleFormData = new FormData();
        singleFormData.append('file', files[i]);
        const res = await axiosInstance.post('/files/upload', singleFormData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        newFileIds.push(res.data.id);
        newPreviewUrls.push(res.data.objectKey);
      }

      setFileIds(newFileIds);
      setPreviewUrls(newPreviewUrls);
      message.success('Đã tải ảnh lên');
    } catch (err) {
      message.error('Lỗi khi tải ảnh');
    } finally {
      setUploading(false);
    }
  };

  const removeImage = (index: number) => {
    setFileIds(fileIds.filter((_, i) => i !== index));
    setPreviewUrls(previewUrls.filter((_, i) => i !== index));
  };

  const selectedService = watch('serviceId');
  const currentRadius = watch('radiusKm');

  const onSubmit = async (data: OpenJobForm) => {
    setLoading(true);
    try {
      const fullAddress = `${data.detailAddress}, ${data.address}`;
      await axiosInstance.post('/open-jobs', { 
        ...data, 
        address: fullAddress,
        fileIds 
      });
      message.success('Đăng bài thành công! Thợ gần bạn sẽ sớm gửi báo giá.');
      navigate('/customer/home');
    } catch (err: any) {
      const errorMsg = err?.response?.data?.message || 'Có lỗi xảy ra khi đăng bài';
      message.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="h-screen bg-gray-50 flex flex-col">
      <div className="bg-white px-4 py-4 flex items-center gap-4 shadow-sm z-10">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 rounded-full hover:bg-gray-100">
          <ArrowLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-lg font-bold text-gray-900">Đăng tin tìm thợ</h1>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 max-w-md mx-auto pb-24">
          
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
              <Wrench className="w-5 h-5 text-orange-500" />
              <h2>Chọn loại dịch vụ</h2>
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
                  <span className="text-xl">⚡</span>
                  <span className="text-sm font-semibold text-gray-700">{srv.name}</span>
                </div>
              ))}
            </div>
            {errors.serviceId && <p className="text-red-500 text-xs mt-2 ml-1">{errors.serviceId.message}</p>}
          </div>

          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
              <FileText className="w-5 h-5 text-orange-500" />
              <h2>Mô tả công việc</h2>
            </div>
            <div className="space-y-4">
                <div>
                    <input 
                        {...register('title')}
                        placeholder="Tiêu đề (Ví dụ: Sửa điện chập âm tường)"
                        className="w-full bg-gray-50 rounded-xl border-none px-4 py-3 text-sm font-semibold text-gray-900 focus:ring-2 focus:ring-orange-500/50"
                    />
                    {errors.title && <p className="text-red-500 text-xs mt-1">{errors.title.message}</p>}
                </div>
                <div>
                    <textarea
                        {...register('description')}
                        placeholder="Mô tả chi tiết tình trạng, yêu cầu..."
                        rows={4}
                        className="w-full bg-gray-50 rounded-2xl border-none px-4 py-3 text-sm text-gray-900 focus:ring-2 focus:ring-orange-500/50 resize-none"
                    />
                    {errors.description && <p className="text-red-500 text-xs mt-1">{errors.description.message}</p>}
                </div>
            </div>
          </div>

          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
              <Upload className="w-5 h-5 text-orange-500" />
              <h2>Ảnh/Video hiện trạng</h2>
            </div>
            
            <div className="grid grid-cols-3 gap-3">
                {previewUrls.map((url, index) => (
                    <div key={index} className="relative aspect-square rounded-xl overflow-hidden group">
                        <img src={`${API_BASE_URL}${url}`} alt="preview" className="w-full h-full object-cover" />
                        <button 
                            type="button"
                            onClick={() => removeImage(index)}
                            className="absolute top-1 right-1 p-1 bg-black/50 rounded-full text-white"
                        >
                            <X className="w-3 h-3" />
                        </button>
                    </div>
                ))}
                {previewUrls.length < 6 && (
                    <label className="aspect-square rounded-xl border-2 border-dashed border-gray-200 flex flex-col items-center justify-center gap-1 cursor-pointer hover:bg-gray-50">
                        <Upload className="w-6 h-6 text-gray-400" />
                        <span className="text-[10px] text-gray-500 font-medium">Thêm ảnh</span>
                        <input type="file" multiple accept="image/*" className="hidden" onChange={handleFileUpload} disabled={uploading} />
                    </label>
                )}
            </div>
            {uploading && <div className="mt-2 flex items-center gap-2 text-xs text-orange-600 font-medium">
                <Loader2 className="w-3 h-3 animate-spin" /> Đang tải ảnh...
            </div>}
          </div>

          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100 space-y-4">
            <div className="flex items-center gap-2 mb-2 text-gray-900 font-bold">
              <DollarSign className="w-5 h-5 text-orange-500" />
              <h2>Ngân sách & Độ ưu tiên</h2>
            </div>
            
            <div className="flex items-center gap-3">
                <div className="flex-1">
                    <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Từ (VNĐ)</label>
                    <InputNumber
                        style={{ width: '100%' }}
                        placeholder="Min"
                        min={0}
                        onChange={(val) => setValue('minBudget', val as number)}
                        formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                        parser={(value) => value!.replace(/\$\s?|(,*)/g, '')}
                        className="rounded-xl border-gray-100 bg-gray-50 font-bold"
                    />
                </div>
                <div className="flex-1">
                    <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Đến (VNĐ)</label>
                    <InputNumber
                        style={{ width: '100%' }}
                        placeholder="Max"
                        min={0}
                        onChange={(val) => setValue('maxBudget', val as number)}
                        formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                        parser={(value) => value!.replace(/\$\s?|(,*)/g, '')}
                        className="rounded-xl border-gray-100 bg-gray-50 font-bold"
                    />
                </div>
            </div>

            <div>
                <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Mức độ ưu tiên</label>
                <Select
                    style={{ width: '100%' }}
                    placeholder="Chọn mức độ ưu tiên"
                    onChange={(val) => setValue('urgencyLevel', val)}
                    className="rounded-xl overflow-hidden"
                    options={[
                        { label: 'Bình thường', value: 'NORMAL' },
                        { label: 'Cần gấp', value: 'URGENT' },
                        { label: 'Rất gấp (Ưu tiên hàng đầu)', value: 'CRITICAL' },
                    ]}
                />
            </div>
          </div>

          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2 text-gray-900 font-bold">
                <Shield className="w-5 h-5 text-orange-500" />
                <h2>Phạm vi & Vị trí</h2>
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
            
            <div className="mb-4">
                <div className="flex justify-between text-xs font-bold text-gray-500 mb-2">
                    <span>Phạm vi tìm thợ</span>
                    <span className="text-orange-600">{currentRadius}km</span>
                </div>
                <input 
                    type="range"
                    min="1"
                    max="20"
                    step="1"
                    {...register('radiusKm', { valueAsNumber: true })}
                    className="w-full h-2 bg-gray-100 rounded-lg appearance-none cursor-pointer accent-orange-500"
                />
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
          </div>

          <div className="flex items-center gap-3 p-4 bg-blue-50 rounded-2xl text-blue-800">
            <Clock className="w-6 h-6 text-blue-500 flex-shrink-0" />
            <p className="text-sm font-medium">Tin của bạn sẽ được gửi tới các thợ trong vòng {currentRadius}km. Thợ sẽ gửi báo giá để bạn lựa chọn.</p>
          </div>
        </form>
      </div>

      <div className="fixed bottom-0 left-0 right-0 p-4 bg-gradient-to-t from-white via-white to-transparent">
        <button
          onClick={handleSubmit(onSubmit)}
          disabled={loading || uploading}
          className="w-full max-w-md mx-auto block py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-xl shadow-orange-500/20 transition-all disabled:opacity-70 flex items-center justify-center gap-2"
        >
          {loading && <Loader2 className="w-5 h-5 animate-spin" />}
          {loading ? 'Đang đăng tin...' : 'Đăng tin ngay'}
        </button>
      </div>
    </div>
  );
}
