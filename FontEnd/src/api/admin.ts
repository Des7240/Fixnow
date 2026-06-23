import axiosInstance from '../utils/axiosInstance';

export interface PaginationParams {
  pageIndex: number;
  pageSize: number;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface GetBookingsQuery extends PaginationParams {
  dateFrom?: string;
  dateTo?: string;
  status?: string;
  searchTerm?: string;
}

export interface GetTransactionsQuery extends PaginationParams {
  dateFrom?: string;
  dateTo?: string;
  type?: string;
  searchTerm?: string;
}

export const adminApi = {
  getBookings: async (params: GetBookingsQuery) => {
    const res = await axiosInstance.get('/admin/bookings', { params });
    return res.data;
  },

  getTransactions: async (params: GetTransactionsQuery) => {
    const res = await axiosInstance.get('/admin/transactions', { params });
    return res.data;
  }
};
