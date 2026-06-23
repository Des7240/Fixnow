import axios from 'axios';
import * as SecureStore from 'expo-secure-store';
import { useAuthStore } from '../stores/useAuthStore';

const API_URL = 'https://fixnow-api-009b.onrender.com/api/v1';

const axiosClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor for requests
axiosClient.interceptors.request.use(
  async (config) => {
    const token = await SecureStore.getItemAsync('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor for responses
axiosClient.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const refreshToken = await SecureStore.getItemAsync('refreshToken');
        if (refreshToken) {
          const res = await axios.post(`${API_URL}/auth/refresh`, {
            refreshToken: refreshToken,
          });
          
          if (res.data && res.data.accessToken) {
            await SecureStore.setItemAsync('accessToken', res.data.accessToken);
            // Optionally update refresh token if a new one is returned
            if (res.data.refreshToken) {
              await SecureStore.setItemAsync('refreshToken', res.data.refreshToken);
            }
            
            originalRequest.headers.Authorization = `Bearer ${res.data.accessToken}`;
            return axiosClient(originalRequest);
          }
        }
      } catch (refreshError) {
        // If refresh fails, clear tokens and redirect to Login
        await SecureStore.deleteItemAsync('accessToken');
        await SecureStore.deleteItemAsync('refreshToken');
        useAuthStore.getState().logout();
        // Cần reload app hoặc dùng navigation ref để chuyển sang màn hình Đăng nhập
      }
    }
    return Promise.reject(error);
  }
);

export default axiosClient;
