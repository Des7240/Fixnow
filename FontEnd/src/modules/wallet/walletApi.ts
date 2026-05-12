import axiosInstance from '../../utils/axiosInstance';

export interface WithdrawRequest {
  amount: number;
  bankName: string;
  accountNumber: string;
  accountName: string;
}

export interface ConfirmWithdrawRequest extends WithdrawRequest {
  otpCode: string;
}

export const walletApi = {
  getWallet: () =>
    axiosInstance.get('/wallet'),

  getTransactions: () =>
    axiosInstance.get('/wallet/transactions'),

  getWithdrawals: () =>
    axiosInstance.get('/wallet/withdrawals'),

  initiateWithdraw: (data: WithdrawRequest) =>
    axiosInstance.post('/wallet/withdraw', data),

  confirmWithdraw: (data: ConfirmWithdrawRequest) =>
    axiosInstance.post('/wallet/confirm-withdraw', data),

  deposit: (data: { amount: number }) =>
    axiosInstance.post('/wallet/deposit', data),
};
