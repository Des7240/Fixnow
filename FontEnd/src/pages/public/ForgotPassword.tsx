import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { message, Steps } from 'antd';
import { Mail, Lock, Zap, KeyRound, ShieldCheck } from 'lucide-react';
import { authApi } from '../../modules/auth/authApi';

const emailSchema = z.object({
  email: z.string().email('Email không hợp lệ'),
});

const otpSchema = z.object({
  code: z.string().length(6, 'Mã OTP phải có 6 chữ số'),
});

const resetPasswordSchema = z.object({
  newPassword: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
  confirmPassword: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: "Mật khẩu xác nhận không khớp",
  path: ["confirmPassword"],
});

type EmailForm = z.infer<typeof emailSchema>;
type OtpForm = z.infer<typeof otpSchema>;
type ResetPasswordForm = z.infer<typeof resetPasswordSchema>;

export default function ForgotPassword() {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [email, setEmail] = useState('');
  const [otp, setOtp] = useState('');

  // Forms
  const emailForm = useForm<EmailForm>({ resolver: zodResolver(emailSchema) });
  const otpForm = useForm<OtpForm>({ resolver: zodResolver(otpSchema) });
  const resetForm = useForm<ResetPasswordForm>({ resolver: zodResolver(resetPasswordSchema) });

  const onSendOtp = async (data: EmailForm) => {
    setLoading(true);
    try {
      await authApi.forgotPassword(data.email);
      setEmail(data.email);
      message.success('Mã OTP đã được gửi về email của bạn');
      setCurrentStep(1);
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Gửi OTP thất bại');
    } finally {
      setLoading(false);
    }
  };

  const onVerifyOtp = async (data: OtpForm) => {
    setLoading(true);
    try {
      await authApi.verifyResetOtp({ email, code: data.code });
      setOtp(data.code);
      message.success('Xác thực OTP thành công');
      setCurrentStep(2);
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Mã OTP không chính xác');
    } finally {
      setLoading(false);
    }
  };

  const onResetPassword = async (data: ResetPasswordForm) => {
    setLoading(true);
    try {
      await authApi.resetPassword({ email, code: otp, newPassword: data.newPassword });
      message.success('Đổi mật khẩu thành công! Hãy đăng nhập lại.');
      navigate('/login');
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Đổi mật khẩu thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-6">
      <div className="w-full max-w-md bg-white rounded-3xl shadow-xl p-8 border border-gray-100">
        <div className="flex items-center gap-2 mb-8 justify-center">
          <div className="w-9 h-9 rounded-xl bg-orange-500 flex items-center justify-center">
            <Zap className="w-5 h-5 text-white" />
          </div>
          <span className="text-xl font-bold text-gray-900">FixNow</span>
        </div>

        <h2 className="text-2xl font-bold text-center text-gray-900 mb-6">Khôi phục mật khẩu</h2>

        <Steps
          current={currentStep}
          size="small"
          className="mb-8"
          items={[
            { title: 'Email' },
            { title: 'OTP' },
            { title: 'Đặt lại' },
          ]}
        />

        {currentStep === 0 && (
          <form onSubmit={emailForm.handleSubmit(onSendOtp)} className="space-y-6">
            <p className="text-sm text-gray-500">Nhập email của bạn để nhận mã xác thực khôi phục mật khẩu.</p>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Email</label>
              <div className="relative">
                <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-gray-400" />
                <input
                  {...emailForm.register('email')}
                  type="email"
                  id="forgot-email"
                  placeholder="ten@email.com"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 bg-white focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {emailForm.formState.errors.email && (
                <p className="text-red-500 text-xs mt-1">{emailForm.formState.errors.email.message}</p>
              )}
            </div>
            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-orange-500 hover:bg-orange-600 text-white font-semibold rounded-xl transition-all flex items-center justify-center gap-2 shadow-lg shadow-orange-500/20"
            >
              {loading && <span className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />}
              Gửi mã xác thực
            </button>
          </form>
        )}

        {currentStep === 1 && (
          <form onSubmit={otpForm.handleSubmit(onVerifyOtp)} className="space-y-6">
            <p className="text-sm text-gray-500">Mã OTP 6 chữ số đã được gửi đến <strong>{email}</strong>. Vui lòng kiểm tra hộp thư.</p>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Mã OTP</label>
              <div className="relative">
                <KeyRound className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-gray-400" />
                <input
                  {...otpForm.register('code')}
                  type="text"
                  id="otp-code"
                  maxLength={6}
                  placeholder="123456"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 bg-white text-center text-lg font-bold tracking-[0.5em] focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {otpForm.formState.errors.code && (
                <p className="text-red-500 text-xs mt-1">{otpForm.formState.errors.code.message}</p>
              )}
            </div>
            <div className="flex flex-col gap-3">
              <button
                type="submit"
                disabled={loading}
                className="w-full py-3.5 bg-orange-500 hover:bg-orange-600 text-white font-semibold rounded-xl transition-all flex items-center justify-center gap-2 shadow-lg shadow-orange-500/20"
              >
                {loading && <span className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />}
                Xác thực mã OTP
              </button>
              <button
                type="button"
                onClick={() => setCurrentStep(0)}
                className="text-sm text-gray-500 hover:text-orange-500 text-center"
              >
                Quay lại nhập email
              </button>
            </div>
          </form>
        )}

        {currentStep === 2 && (
          <form onSubmit={resetForm.handleSubmit(onResetPassword)} className="space-y-5">
            <p className="text-sm text-gray-500">Xác thực thành công. Vui lòng đặt mật khẩu mới cho tài khoản của bạn.</p>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Mật khẩu mới</label>
              <div className="relative">
                <Lock className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-gray-400" />
                <input
                  {...resetForm.register('newPassword')}
                  type="password"
                  id="reset-password"
                  placeholder="••••••••"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 bg-white focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {resetForm.formState.errors.newPassword && (
                <p className="text-red-500 text-xs mt-1">{resetForm.formState.errors.newPassword.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Xác nhận mật khẩu</label>
              <div className="relative">
                <ShieldCheck className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-gray-400" />
                <input
                  {...resetForm.register('confirmPassword')}
                  type="password"
                  id="confirm-password"
                  placeholder="••••••••"
                  className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 bg-white focus:outline-none focus:ring-2 focus:ring-orange-500/30 focus:border-orange-500 transition-all"
                />
              </div>
              {resetForm.formState.errors.confirmPassword && (
                <p className="text-red-500 text-xs mt-1">{resetForm.formState.errors.confirmPassword.message}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-orange-500 hover:bg-orange-600 text-white font-semibold rounded-xl transition-all flex items-center justify-center gap-2 shadow-lg shadow-orange-500/20"
            >
              {loading && <span className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />}
              Cập nhật mật khẩu
            </button>
          </form>
        )}

        <p className="text-center mt-8">
          <Link to="/login" className="text-sm text-gray-500 hover:text-orange-500 font-medium transition-colors">
            Quay lại đăng nhập
          </Link>
        </p>
      </div>
    </div>
  );
}
