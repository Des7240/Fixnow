import axiosClient from './axiosClient';

export const authApi = {
  login: (data: any) => {
    return axiosClient.post('/auth/login', data);
  },
  getProfile: () => {
    return axiosClient.get('/auth/me');
  }
};
