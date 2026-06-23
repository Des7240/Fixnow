import axiosClient from './axiosClient';

export const quotationApi = {
  createQuotation: (data: { bookingId: string, items: { itemName: string, quantity: number, unitPrice: number }[], note?: string }) => {
    return axiosClient.post('/quotations', data);
  },
  getQuotation: (id: string) => {
    return axiosClient.get(`/quotations/${id}`);
  },
  getQuotationsByBooking: (bookingId: string) => {
    return axiosClient.get(`/quotations/booking/${bookingId}`);
  }
};
