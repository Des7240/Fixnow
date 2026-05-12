import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { message } from 'antd';
import { Mail, Lock, Zap } from 'lucide-react';
import { authApi } from '../../modules/auth/authApi';
import { useAuthStore } from '../../stores/authStore';
import GoogleLoginButton from '../../components/GoogleLoginButton';

const loginSchema = z.object({
  email: z.string().email('Email không hợp lệ'),
  password: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function Login() {
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [loading, setLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginForm) => {
    setLoading(true);
    try {
      const res = await authApi.login(data);
      const { user, accessToken } = res.data;
      setAuth({ ...user, role: user.role as 'CUSTOMER' | 'WORKER' | 'ADMIN' }, accessToken);

      if (user.role === 'WORKER') navigate('/worker');
      else if (user.role === 'ADMIN') navigate('/admin');
      else navigate('/');
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      message.error(error?.response?.data?.message || 'Đăng nhập thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex">
      {/* Left branding panel */}
      <div className="hidden lg:flex flex-col justify-between w-1/2 bg-gradient-to-br from-orange-500 via-orange-600 to-amber-700 p-12 text-white">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-white/20 backdrop-blur flex items-center justify-center">
            <Zap className="w-6 h-6 text-white" />
          </div>
          <span className="text-2xl font-bold tracking-tight">FixNow</span>
        </div>
        <div>
          <h1 className="text-5xl font-extrabold leading-tight mb-6">
            Dịch vụ<br />tại nhà<br />tức thì
          </h1>
          <p className="text-orange-100 text-lg max-w-sm">
            Kết nối với hàng nghìn thợ lành nghề trong vài giây. Nhanh, an toàn, đáng tin cậy.
          </p>
        </div>
        <div className="flex gap-4">
          <div className="bg-white/10 backdrop-blur rounded-2xl p-4 flex-1 text-center">
            <p className="text-3xl font-bold">1,200+</p>
            <p className="text-orange-100 text-sm mt-1">Thợ lành nghề</p>
          </div>
          <div className="bg-white/10 backdrop-blur rounded-2xl p-4 flex-1 text-center">
            <p className="text-3xl font-bold">98%</p>
            <p className="text-orange-100 text-sm mt-1">Hài lòng</p>
          </div>
          <div className="bg-white/10 backdrop-blur rounded-2xl p-4 flex-1 text-center">
            <p className="text-3xl font-bold">15p</p>
            <p className="text-orange-100 text-sm mt-1">Thời gian phản hồi</p>
          </div>
        </div>
      </div>

      {/* Right form panel */}
      <div className="flex-1 flex items-center justify-center p-8 bg-gray-50">
        <div className="w-full max-w-md">
          {/* Mobile logo */}
          <div className="flex lg:hidden items-center gap-2 mb-8">
            <div className="w-9 h-9 rounded-xl bg-orange-500 flex items-center justify-center">
              <Zap className="w-5 h-5 text-white" />
            </div>
            <span className="text-xl font-bold text-gray-900">FixNow</span>
          </div>

          <h2 className="text-3xl font-bold text-gray-900 mb-2">Chào mừng trở lại</h2>
          <p className="text-gray-500 mb-8">Đăng nhập để tiếp tục sử dụng dịch vụ</p>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            {/* Email */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Email</label>
              <div className="relative">
                <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-gray-400" />
                <input
                  {...register('email')}
                  type="email"
                  id="login-email"
                  placeholder="ten@email.com"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
            </div>

            {/* Password */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Mật khẩu</label>
              <div className="relative">
                <Lock className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-gray-400" />
                <input
                  {...register('password')}
                  type="password"
                  id="login-password"
                  placeholder="••••••••"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
              <div className="flex justify-end mt-1">
                <Link to="/forgot-password" id="forgot-password-link" className="text-xs text-orange-500 hover:underline">
                  Quên mật khẩu?
                </Link>
              </div>
            </div>

            {/* Submit */}
            <button
              id="login-submit"
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-orange-500 hover:bg-orange-600 active:bg-orange-700 text-white font-semibold rounded-xl transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed flex items-center justify-center gap-2 shadow-lg shadow-orange-500/25"
            >
              {loading ? (
                <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
              ) : null}
              {loading ? 'Đang đăng nhập...' : 'Đăng nhập'}
            </button>

            <div className="relative my-6">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-200"></div>
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-gray-50 text-gray-500 uppercase tracking-wider text-xs font-medium">Hoặc đăng nhập với</span>
              </div>
            </div>

            <GoogleLoginButton />
          </form>

          <p className="text-center text-gray-500 text-sm mt-8">
            Chưa có tài khoản?{' '}
            <Link to="/register" className="text-orange-500 font-semibold hover:underline">
              Đăng ký ngay
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
