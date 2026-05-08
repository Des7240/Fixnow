import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { User, Briefcase, Wrench, Save } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { useAuthStore } from '../../stores/authStore';

const SERVICES = [
  { id: '1', name: 'Sửa điện', icon: '⚡' },
  { id: '2', name: 'Sửa nước', icon: '💧' },
  { id: '3', name: 'Sửa điều hoà', icon: '❄️' },
  { id: '4', name: 'Sửa khoá', icon: '🔑' },
];

export default function WorkerProfile() {
  const { user } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const [selectedSkills, setSelectedSkills] = useState<string[]>([]);
  const { register, handleSubmit, setValue } = useForm();

  useEffect(() => {
    // Fetch current profile
    const fetchProfile = async () => {
      try {
        const res = await axiosInstance.get('/workers/profile');
        if (res.data) {
          setValue('bio', res.data.bio);
          setValue('experienceYears', res.data.experienceYears);
          if (res.data.skills) {
            setSelectedSkills(res.data.skills.map((s: any) => s.serviceId));
          }
        }
      } catch (err) {
        // First time visiting profile
      }
    };
    fetchProfile();
  }, [setValue]);

  const toggleSkill = (id: string) => {
    setSelectedSkills(prev => 
      prev.includes(id) ? prev.filter(s => s !== id) : [...prev, id]
    );
  };

  const onSubmit = async (data: any) => {
    setLoading(true);
    try {
      // 1. Update Profile Info
      await axiosInstance.post('/workers/profile', {
        bio: data.bio,
        experienceYears: parseInt(data.experienceYears) || 0
      });

      // 2. Update Skills
      await axiosInstance.post('/workers/profile/skills', {
        serviceIds: selectedSkills
      });

      message.success('Cập nhật hồ sơ thành công!');
    } catch (err) {
      message.error('Có lỗi xảy ra khi cập nhật');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-full bg-gray-50 flex flex-col">
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm z-10 sticky top-0 flex items-center gap-3">
        <User className="w-6 h-6 text-orange-500" />
        <h1 className="text-2xl font-bold text-gray-900">Hồ sơ Của Tôi</h1>
      </div>

      <div className="flex-1 p-6 overflow-y-auto pb-24">
        <div className="flex items-center gap-4 bg-white p-5 rounded-3xl shadow-sm border border-gray-100 mb-6">
          <div className="w-16 h-16 bg-orange-100 rounded-full flex items-center justify-center text-orange-600 font-bold text-2xl">
            {user?.fullName.charAt(0)}
          </div>
          <div>
            <h2 className="text-lg font-bold text-gray-900">{user?.fullName}</h2>
            <p className="text-gray-500 text-sm">{user?.email}</p>
          </div>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <h3 className="flex items-center gap-2 font-bold text-gray-900 mb-4">
              <Briefcase className="w-5 h-5 text-orange-500" /> Giới thiệu bản thân
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Kinh nghiệm (Năm)</label>
                <input
                  {...register('experienceYears')}
                  type="number"
                  min="0"
                  className="w-full bg-gray-50 rounded-xl px-4 py-3 text-gray-900 focus:outline-none focus:ring-2 focus:ring-orange-500/50"
                  placeholder="Ví dụ: 3"
                />
              </div>
              
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Mô tả ngắn</label>
                <textarea
                  {...register('bio')}
                  rows={3}
                  className="w-full bg-gray-50 rounded-xl px-4 py-3 text-gray-900 focus:outline-none focus:ring-2 focus:ring-orange-500/50 resize-none"
                  placeholder="Ví dụ: Chuyên sửa điện nước gia đình, nhiệt tình, đúng hẹn."
                />
              </div>
            </div>
          </div>

          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <h3 className="flex items-center gap-2 font-bold text-gray-900 mb-4">
              <Wrench className="w-5 h-5 text-orange-500" /> Kỹ năng chuyên môn
            </h3>
            <p className="text-xs text-gray-500 mb-4">Hệ thống sẽ chỉ phát đơn phù hợp với kỹ năng bạn chọn.</p>
            
            <div className="grid grid-cols-2 gap-3">
              {SERVICES.map(srv => (
                <div 
                  key={srv.id}
                  onClick={() => toggleSkill(srv.id)}
                  className={`flex flex-col items-center p-3 rounded-xl border-2 transition-all cursor-pointer ${
                    selectedSkills.includes(srv.id)
                      ? 'border-orange-500 bg-orange-50 text-orange-700'
                      : 'border-gray-100 bg-gray-50 text-gray-600'
                  }`}
                >
                  <span className="text-2xl mb-1">{srv.icon}</span>
                  <span className="text-xs font-semibold">{srv.name}</span>
                </div>
              ))}
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl shadow-xl shadow-gray-900/20 transition-all disabled:opacity-70 flex items-center justify-center gap-2"
          >
            {loading ? 'Đang lưu...' : (
              <>
                <Save className="w-5 h-5" /> Lưu Thay Đổi
              </>
            )}
          </button>
        </form>
      </div>
    </div>
  );
}
