import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { message } from 'antd';
import { Mail, Lock, User, Phone, Zap, Briefcase, UserCheck } from 'lucide-react';
import { authApi } from '../../modules/auth/authApi';
import { useAuthStore } from '../../stores/authStore';
import { clsx } from 'clsx';

const registerSchema = z.object({
  fullName: z.string().min(2, 'Họ tên tối thiểu 2 ký tự'),
  email: z.string().email('Email không hợp lệ'),
  password: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
  confirmPassword: z.string(),
  phoneNumber: z.string().optional(),
  role: z.enum(['CUSTOMER', 'WORKER']),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Mật khẩu xác nhận không khớp',
  path: ['confirmPassword'],
});

type RegisterForm = z.infer<typeof registerSchema>;

export default function Register() {
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [loading, setLoading] = useState(false);

  const { register, handleSubmit, setValue, watch, formState: { errors } } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
    defaultValues: { role: 'CUSTOMER' },
  });

  const selectedRole = watch('role');

  const onSubmit = async (data: RegisterForm) => {
    setLoading(true);
    try {
      const { confirmPassword, ...payload } = data;
      void confirmPassword;
      const res = await authApi.register(payload);
      const { user, accessToken } = res.data;
      setAuth({ ...user, role: user.role as 'CUSTOMER' | 'WORKER' | 'ADMIN' }, accessToken);
      message.success('Đăng ký thành công!');

      if (user.role === 'WORKER') navigate('/worker');
      else navigate('/');
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      message.error(error?.response?.data?.message || 'Đăng ký thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 via-orange-50 to-amber-50 p-4">
      <div className="w-full max-w-lg bg-white rounded-3xl shadow-xl shadow-gray-200/80 p-8 md:p-10">
        {/* Logo */}
        <div className="flex items-center gap-2 mb-8">
          <div className="w-10 h-10 rounded-xl bg-orange-500 flex items-center justify-center">
            <Zap className="w-6 h-6 text-white" />
          </div>
          <span className="text-2xl font-bold text-gray-900">FixNow</span>
        </div>

        <h2 className="text-2xl font-bold text-gray-900 mb-1">Tạo tài khoản mới</h2>
        <p className="text-gray-500 mb-6">Tham gia cộng đồng FixNow ngay hôm nay</p>

        {/* Role selector */}
        <div className="grid grid-cols-2 gap-3 mb-6">
          <button
            id="role-customer"
            type="button"
            onClick={() => setValue('role', 'CUSTOMER')}
            className={clsx(
              'flex flex-col items-center gap-2 p-4 rounded-2xl border-2 transition-all duration-200',
              selectedRole === 'CUSTOMER'
                ? 'border-orange-500 bg-orange-50 text-orange-600'
                : 'border-gray-200 bg-white text-gray-500 hover:border-gray-300'
            )}
          >
            <UserCheck className="w-6 h-6" />
            <span className="text-sm font-semibold">Khách hàng</span>
          </button>
          <button
            id="role-worker"
            type="button"
            onClick={() => setValue('role', 'WORKER')}
            className={clsx(
              'flex flex-col items-center gap-2 p-4 rounded-2xl border-2 transition-all duration-200',
              selectedRole === 'WORKER'
                ? 'border-orange-500 bg-orange-50 text-orange-600'
                : 'border-gray-200 bg-white text-gray-500 hover:border-gray-300'
            )}
          >
            <Briefcase className="w-6 h-6" />
            <span className="text-sm font-semibold">Thợ dịch vụ</span>
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Full Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Họ và tên</label>
            <div className="relative">
              <User className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                {...register('fullName')}
                id="reg-fullname"
                type="text"
                placeholder="Nguyễn Văn A"
                className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
              />
            </div>
            {errors.fullName && <p className="text-red-500 text-xs mt-1">{errors.fullName.message}</p>}
          </div>

          {/* Email */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Email</label>
            <div className="relative">
              <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                {...register('email')}
                id="reg-email"
                type="email"
                placeholder="ten@email.com"
                className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
              />
            </div>
            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
          </div>

          {/* Phone */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Số điện thoại <span className="text-gray-400">(tuỳ chọn)</span></label>
            <div className="relative">
              <Phone className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                {...register('phoneNumber')}
                id="reg-phone"
                type="tel"
                placeholder="0912 345 678"
                className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
              />
            </div>
          </div>

          {/* Password */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Mật khẩu</label>
              <div className="relative">
                <Lock className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  {...register('password')}
                  id="reg-password"
                  type="password"
                  placeholder="••••••••"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Xác nhận</label>
              <div className="relative">
                <Lock className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  {...register('confirmPassword')}
                  id="reg-confirm-password"
                  type="password"
                  placeholder="••••••••"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {errors.confirmPassword && <p className="text-red-500 text-xs mt-1">{errors.confirmPassword.message}</p>}
            </div>
          </div>

          {/* Submit */}
          <button
            id="register-submit"
            type="submit"
            disabled={loading}
            className="w-full py-3.5 bg-orange-500 hover:bg-orange-600 text-white font-semibold rounded-xl transition-all duration-200 disabled:opacity-60 flex items-center justify-center gap-2 shadow-lg shadow-orange-500/25 mt-2"
          >
            {loading && <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />}
            {loading ? 'Đang tạo tài khoản...' : 'Tạo tài khoản'}
          </button>
        </form>

        <p className="text-center text-gray-500 text-sm mt-6">
          Đã có tài khoản?{' '}
          <Link to="/login" className="text-orange-500 font-semibold hover:underline">
            Đăng nhập
          </Link>
        </p>
      </div>
    </div>
  );
}
