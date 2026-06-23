import axiosClient from './axiosClient';

export const workerApi = {
  getProfile: () => {
    return axiosClient.get('/workers/profile');
  },
  updateProfile: (data: any) => {
    return axiosClient.post('/workers/profile', data);
  },
  updateAvailability: (status: 'ONLINE' | 'OFFLINE') => {
    return axiosClient.patch('/workers/profile/availability', { status });
  },
  getWallet: () => {
    return axiosClient.get('/wallet');
  },
  getTransactions: () => {
    return axiosClient.get('/wallet/transactions');
  },
  requestWithdrawal: (amount: number, bankDetails: string) => {
    return axiosClient.post('/wallet/withdraw', { amount, bankDetails });
  },
  getKycStatus: () => {
    return axiosClient.get('/kyc/me');
  },
  submitKyc: (formData: FormData) => {
    return axiosClient.post('/kyc', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
  }
};
