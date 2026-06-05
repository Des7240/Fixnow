import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { User, Briefcase, Wrench, Save, Star, MessageSquare, LogOut, Lock, Shield, FileBadge, ChevronRight } from 'lucide-react';
import { message, Modal, Form, Input } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { useAuthStore } from '../../stores/authStore';
import { clsx } from 'clsx';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../../modules/auth/authApi';
import { getImageUrl } from '../../utils/constants';

interface Review {
  id: string;
  customerName: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export default function WorkerProfile() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [selectedSkills, setSelectedSkills] = useState<string[]>([]);
  const [skillStatuses, setSkillStatuses] = useState<Record<string, string>>({});
  const [availableServices, setAvailableServices] = useState<{id: string, name: string, iconUrl?: string}[]>([]);
  const [ratingSummary, setRatingSummary] = useState({ averageRating: 0, totalReviews: 0 });
  const [reviews, setReviews] = useState<Review[]>([]);
  const { register, handleSubmit, setValue, formState: { errors } } = useForm();

  // Password Modal
  const [isPasswordModalVisible, setIsPasswordModalVisible] = useState(false);
  const [passwordForm] = Form.useForm();
  const [passwordLoading, setPasswordLoading] = useState(false);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const srvRes = await axiosInstance.get('/services');
        setAvailableServices(srvRes.data);

        const res = await axiosInstance.get('/workers/profile');
        if (res.data) {
          setValue('phoneNumber', res.data.phoneNumber);
          setValue('bio', res.data.bio);
          setValue('experienceYears', res.data.experienceYears);
          if (res.data.skills) {
            setSelectedSkills(res.data.skills.map((s: any) => s.serviceId));
            const statuses: Record<string, string> = {};
            res.data.skills.forEach((s: any) => {
              statuses[s.serviceId] = s.status;
            });
            setSkillStatuses(statuses);
          }
          if (user?.id) {
            fetchReviews(user.id);
          }
        }
      } catch (err) {
        // First time
      }
    };
    
    const fetchReviews = async (workerId: string) => {
      try {
        const [summaryRes, reviewsRes] = await Promise.all([
          axiosInstance.get(`/reviews/workers/${workerId}/summary`),
          axiosInstance.get(`/reviews/workers/${workerId}`)
        ]);
        setRatingSummary(summaryRes.data);
        setReviews(reviewsRes.data);
      } catch (err) {
        console.error('Lỗi tải đánh giá');
      }
    };

    fetchProfile();
  }, [setValue, user?.id]);

  const toggleSkill = (id: string) => {
    setSelectedSkills(prev => {
      const isSelecting = !prev.includes(id);
      if (isSelecting && skillStatuses[id] === 'REJECTED') {
        // Clear rejected status locally to show as pending when re-selecting
        setSkillStatuses(prevStatuses => ({ ...prevStatuses, [id]: 'PENDING' }));
      }
      return isSelecting ? [...prev, id] : prev.filter(s => s !== id);
    });
  };

  const onSubmit = async (data: any) => {
    setLoading(true);
    try {
      const profileRes = await axiosInstance.post('/workers/profile', {
        phoneNumber: data.phoneNumber,
        bio: data.bio,
        experienceYears: parseInt(data.experienceYears) || 0
      });

      // Update auth store with new phone number
      if (profileRes.data && user) {
        useAuthStore.getState().setUser({
          ...user,
          phoneNumber: profileRes.data.phoneNumber
        });
      }

      const res = await axiosInstance.post('/workers/profile/skills', {
        serviceIds: selectedSkills
      });
      
      if (res.data && res.data.skills) {
        setSelectedSkills(res.data.skills.map((s: any) => s.serviceId));
        const statuses: Record<string, string> = {};
        res.data.skills.forEach((s: any) => {
          statuses[s.serviceId] = s.status;
        });
        setSkillStatuses(statuses);
      }

      message.success('Cập nhật hồ sơ thành công!');
    } catch (err) {
      message.error('Có lỗi xảy ra khi cập nhật');
    } finally {
      setLoading(false);
    }
  };

  const handleChangePassword = async (values: any) => {
    setPasswordLoading(true);
    try {
      await authApi.changePassword({
        oldPassword: values.oldPassword,
        newPassword: values.newPassword
      });
      message.success('Đổi mật khẩu thành công!');
      setIsPasswordModalVisible(false);
      passwordForm.resetFields();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Mật khẩu cũ không chính xác.');
    } finally {
      setPasswordLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
        await authApi.logout();
    } catch (error) {
        console.error('Logout error', error);
    } finally {
        logout();
        navigate('/login');
    }
  };

  return (
    <div className="min-h-full bg-gray-50 flex flex-col">
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm z-10 sticky top-0 flex items-center gap-3">
        <User className="w-6 h-6 text-orange-500" />
        <h1 className="text-2xl font-bold text-gray-900">Hồ sơ Của Tôi</h1>
      </div>

      <div className="flex-1 p-6 overflow-y-auto pb-24">
        <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100 mb-6 flex flex-col items-center">
          <div className="w-20 h-20 bg-orange-100 rounded-full flex items-center justify-center text-orange-600 font-bold text-3xl mb-4">
            {user?.fullName.charAt(0)}
          </div>
          <h2 className="text-xl font-bold text-gray-900">{user?.fullName}</h2>
          <p className="text-gray-500 text-sm mb-4">{user?.email}</p>
          
          <div className="flex items-center gap-6 w-full pt-4 border-t border-gray-50">
            <div className="flex-1 text-center">
              <div className="flex items-center justify-center gap-1 text-orange-500 font-bold text-lg">
                <Star className="w-5 h-5 fill-current" />
                {ratingSummary.averageRating.toFixed(1)}
              </div>
              <p className="text-[10px] text-gray-400 font-bold uppercase">Điểm đánh giá</p>
            </div>
            <div className="w-px h-8 bg-gray-100"></div>
            <div className="flex-1 text-center">
              <div className="text-gray-900 font-bold text-lg">
                {ratingSummary.totalReviews}
              </div>
              <p className="text-[10px] text-gray-400 font-bold uppercase">Nhận xét</p>
            </div>
          </div>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <h3 className="flex items-center gap-2 font-bold text-gray-900 mb-4">
              <Briefcase className="w-5 h-5 text-orange-500" /> Thông tin cơ bản
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Số điện thoại <span className="text-red-500">*</span></label>
                <input
                  {...register('phoneNumber', { 
                    required: 'Số điện thoại là bắt buộc',
                    pattern: {
                      value: /^(0[3|5|7|8|9])+([0-9]{8})$/,
                      message: 'Số điện thoại Việt Nam không hợp lệ'
                    }
                  })}
                  type="tel"
                  className="w-full bg-gray-50 rounded-xl px-4 py-3 text-gray-900 focus:outline-none focus:ring-2 focus:ring-orange-500/50"
                  placeholder="Ví dụ: 0987654321"
                />
                {errors.phoneNumber && <p className="text-red-500 text-[10px] mt-1 ml-1">{(errors.phoneNumber as any).message}</p>}
              </div>

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
              {availableServices.map(srv => {
                const isSelected = selectedSkills.includes(srv.id);
                const status = skillStatuses[srv.id] || 'PENDING';
                
                return (
                  <div 
                    key={srv.id}
                    onClick={() => toggleSkill(srv.id)}
                    className={clsx(
                      "relative flex flex-col items-center p-3 rounded-xl border-2 transition-all cursor-pointer",
                      isSelected
                        ? status === 'APPROVED' 
                          ? "border-green-500 bg-green-50 text-green-700"
                          : status === 'REJECTED'
                            ? "border-red-500 bg-red-50 text-red-700"
                            : "border-orange-500 bg-orange-50 text-orange-700"
                        : "border-gray-100 bg-gray-50 text-gray-600"
                    )}
                  >
                    {isSelected && (
                      <div className={clsx(
                        "absolute top-2 right-2 text-[10px] px-1.5 py-0.5 rounded font-bold uppercase",
                        status === 'APPROVED' ? "bg-green-100 text-green-700" :
                        status === 'REJECTED' ? "bg-red-100 text-red-700" :
                        "bg-orange-100 text-orange-700"
                      )}>
                        {status === 'APPROVED' ? 'Đã duyệt' : status === 'REJECTED' ? 'Từ chối' : 'Đang chờ'}
                      </div>
                    )}
                    <span className="text-2xl mb-1 mt-2">{srv.iconUrl ? <img src={getImageUrl(srv.iconUrl)} alt={srv.name} className="w-8 h-8 rounded-full" /> : '⚡'}</span>
                    <span className="text-xs font-semibold">{srv.name}</span>
                  </div>
                );
              })}
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

        <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100 mt-6 cursor-pointer hover:bg-gray-50 transition-colors"
             onClick={() => setIsPasswordModalVisible(true)}>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-orange-50 flex items-center justify-center text-orange-500">
                <Shield size={20} />
              </div>
              <div>
                <p className="text-sm font-bold text-gray-900">Đổi mật khẩu</p>
                <p className="text-xs text-gray-500">Cập nhật mật khẩu bảo vệ tài khoản</p>
              </div>
            </div>
            <ChevronRight size={18} className="text-gray-400" />
          </div>
        </div>

        <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100 mt-4 cursor-pointer hover:bg-gray-50 transition-colors"
             onClick={() => navigate('/worker/kyc')}>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-600">
                <FileBadge size={20} />
              </div>
              <div>
                <p className="text-sm font-bold text-gray-900">Hồ sơ định danh (KYC)</p>
                <p className="text-xs text-gray-500">Cập nhật giấy tờ tùy thân, bằng cấp để nhận việc</p>
              </div>
            </div>
            <ChevronRight size={18} className="text-gray-400" />
          </div>
        </div>

        <div className="mt-8">
          <h3 className="flex items-center gap-2 font-bold text-gray-900 mb-4 px-2">
            <MessageSquare className="w-5 h-5 text-orange-500" /> Đánh giá từ khách hàng
          </h3>
          
          {reviews.length === 0 ? (
            <div className="bg-white p-8 rounded-3xl border border-dashed border-gray-200 text-center text-gray-400">
              <Star className="w-10 h-10 mx-auto mb-2 opacity-20" />
              <p className="text-sm">Chưa có đánh giá nào</p>
            </div>
          ) : (
            <div className="space-y-4">
              {reviews.map(review => (
                <div key={review.id} className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
                  <div className="flex justify-between items-start mb-2">
                    <div className="flex items-center gap-2">
                      <div className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center text-gray-500 font-bold text-xs">
                        {review.customerName.charAt(0)}
                      </div>
                      <div>
                        <p className="text-xs font-bold text-gray-900">{review.customerName}</p>
                        <p className="text-[10px] text-gray-400">{new Date(review.createdAt).toLocaleDateString('vi-VN')}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-0.5 text-orange-500">
                      {[...Array(5)].map((_, i) => (
                        <Star key={i} className={clsx("w-3 h-3", i < review.rating ? "fill-current" : "text-gray-200")} />
                      ))}
                    </div>
                  </div>
                  <p className="text-sm text-gray-600 mt-2 italic">"{review.comment}"</p>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Nút đăng xuất */}
        <div className="mt-8 mb-6">
          <button 
              onClick={handleLogout}
              className="w-full bg-white border border-red-200 text-red-600 font-semibold py-3.5 rounded-xl shadow-sm flex justify-center items-center gap-2 hover:bg-red-50 transition-colors"
          >
              <LogOut size={20} />
              Đăng xuất
          </button>
        </div>
        
      </div>

      {/* Change Password Modal */}
      <Modal
          title={
              <div className="flex items-center gap-2">
                  <Lock size={20} className="text-orange-500" />
                  <span>Đổi mật khẩu</span>
              </div>
          }
          open={isPasswordModalVisible}
          onCancel={() => {
              setIsPasswordModalVisible(false);
              passwordForm.resetFields();
          }}
          onOk={() => passwordForm.submit()}
          confirmLoading={passwordLoading}
          okText="Cập nhật"
          cancelText="Hủy"
          centered
          className="rounded-2xl overflow-hidden"
      >
          <Form
              form={passwordForm}
              layout="vertical"
              onFinish={handleChangePassword}
              className="mt-4"
          >
              <Form.Item
                  name="oldPassword"
                  label="Mật khẩu hiện tại"
                  rules={[{ required: true, message: 'Vui lòng nhập mật khẩu hiện tại' }]}
              >
                  <Input.Password placeholder="********" className="rounded-lg py-2" />
              </Form.Item>

              <Form.Item
                  name="newPassword"
                  label="Mật khẩu mới"
                  rules={[
                      { required: true, message: 'Vui lòng nhập mật khẩu mới' },
                      { min: 6, message: 'Mật khẩu phải từ 6 ký tự trở lên' }
                  ]}
              >
                  <Input.Password placeholder="********" className="rounded-lg py-2" />
              </Form.Item>

              <Form.Item
                  name="confirmPassword"
                  label="Xác nhận mật khẩu mới"
                  dependencies={['newPassword']}
                  rules={[
                      { required: true, message: 'Vui lòng xác nhận mật khẩu mới' },
                      ({ getFieldValue }) => ({
                          validator(_, value) {
                              if (!value || getFieldValue('newPassword') === value) {
                                  return Promise.resolve();
                              }
                              return Promise.reject(new Error('Mật khẩu xác nhận không khớp!'));
                          },
                      }),
                  ]}
              >
                  <Input.Password placeholder="********" className="rounded-lg py-2" />
              </Form.Item>
          </Form>
      </Modal>
    </div>
  );
}
