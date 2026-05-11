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

  refreshToken: () =>
    axiosInstance.post<{ accessToken: string }>('/auth/refresh'),
};
