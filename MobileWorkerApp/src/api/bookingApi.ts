import axiosClient from './axiosClient';

export const bookingApi = {
  getMyBookings: () => {
    return axiosClient.get('/bookings');
  },
  getMatchingBookings: () => {
    return axiosClient.get('/bookings/matching');
  },
  getBookingDetails: (id: string) => {
    return axiosClient.get(`/bookings/${id}`);
  },
  acceptBooking: (id: string) => {
    return axiosClient.post(`/bookings/${id}/accept`);
  },
  rejectBooking: (id: string) => {
    return axiosClient.post(`/bookings/${id}/reject`);
  },
  updateStatus: (id: string, status: string) => {
    return axiosClient.patch(`/bookings/${id}/status`, { status });
  },
  getTimeline: (id: string) => {
    return axiosClient.get(`/bookings/${id}/timeline`);
  }
};
