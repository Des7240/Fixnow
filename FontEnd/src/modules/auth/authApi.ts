import axiosInstance from '../../utils/axiosInstance';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: 'CUSTOMER' | 'WORKER';
  phoneNumber?: string;
}

export interface AuthResponse {
  accessToken: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    avatarUrl?: string;
    role: string;
    phoneNumber?: string;
    needsPasswordReset: boolean;
  };
}

export const authApi = {
  login: (data: LoginRequest) =>
    axiosInstance.post<AuthResponse>('/auth/login', data),

  register: (data: RegisterRequest) =>
    axiosInstance.post<AuthResponse>('/auth/register', data),

  logout: () =>
    axiosInstance.post('/auth/logout'),

  changePassword: (data: any) =>
    axiosInstance.post('/auth/change-password', data),

  googleLogin: (idToken: string) =>
    axiosInstance.post<AuthResponse>('/auth/google-login', { idToken }),

  forgotPassword: (email: string) =>
    axiosInstance.post('/auth/forgot-password', { email }),

  verifyResetOtp: (data: { email: string; code: string }) =>
    axiosInstance.post('/auth/verify-reset-otp', data),

  resetPassword: (data: any) =>
    axiosInstance.post('/auth/reset-password', data),

  refreshToken: () =>
    axiosInstance.post<{ accessToken: string }>('/auth/refresh'),

  updateProfile: (data: { fullName: string; phoneNumber?: string; avatarUrl?: string }) =>
    axiosInstance.put<AuthResponse>('/auth/profile', data),
};
