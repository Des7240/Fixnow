import axios from 'axios';
import { useAuthStore } from '../stores/authStore';

const axiosInstance = axios.create({
  baseURL: 'http://localhost:5000/api/v1', // Update port if your backend runs on a different port
  timeout: 10000,
});

axiosInstance.interceptors.request.use(
  (config) => {
    const token = useAuthStore.getState().accessToken;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    // Check if error is 401 and we haven't retried yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        // Attempt to refresh token
        const response = await axios.post(
          'http://localhost:5000/api/v1/auth/refresh',
          {},
          { withCredentials: true } // Send httpOnly cookie
        );
        const { accessToken } = response.data;
        useAuthStore.getState().setAccessToken(accessToken);
        
        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return axiosInstance(originalRequest);
      } catch (refreshError) {
        useAuthStore.getState().logout();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
