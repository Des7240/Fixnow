import React from 'react';
import { GoogleLogin } from '@react-oauth/google';
import { authApi } from '../modules/auth/authApi';
import { useAuthStore } from '../stores/authStore';
import { useNavigate } from 'react-router-dom';
import { message } from 'antd';

const GoogleLoginButton: React.FC = () => {
  const setAuth = useAuthStore((state) => state.setAuth);
  const navigate = useNavigate();

  const handleSuccess = async (credentialResponse: any) => {
    try {
      const response = await authApi.googleLogin(credentialResponse.credential);
      const { user, accessToken } = response.data;
      
      setAuth(user as any, accessToken);
      message.success('Đăng nhập Google thành công!');
      
      if (user.needsPasswordReset) {
        navigate('/setup-password');
      } else if (user.role === 'WORKER') {
        navigate('/worker');
      } else if (user.role === 'ADMIN') {
        navigate('/admin');
      } else {
        navigate('/');
      }
    } catch (error) {
      console.error('Google login error:', error);
      message.error('Đăng nhập Google thất bại. Vui lòng thử lại.');
    }
  };

  return (
    <div className="flex justify-center mt-4">
      <GoogleLogin
        onSuccess={handleSuccess}
        onError={() => {
          message.error('Không thể kết nối với Google.');
        }}
        useOneTap
        theme="outline"
        shape="rectangular"
        locale="vi"
      />
    </div>
  );
};

export default GoogleLoginButton;
