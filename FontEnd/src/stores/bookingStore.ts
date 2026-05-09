import { create } from 'zustand';

export type BookingStatus = 
  | 'PENDING'
  | 'MATCHING'
  | 'QUOTED'
  | 'WORKING'
  | 'COMPLETED'
  | 'CANCELLED'
  | 'DISPUTED';

export type BookingPaymentStatus = 'UNPAID' | 'PAID' | 'REFUNDED';

export interface BookingState {
  id: string;
  status: BookingStatus;
  paymentStatus: BookingPaymentStatus;
  serviceId: string;
  customerId: string;
  workerId?: string;
  totalAmount?: number;
}

interface BookingStore {
  currentBooking: BookingState | null;
  setCurrentBooking: (booking: BookingState) => void;
  updateBookingStatus: (status: BookingStatus) => void;
  updatePaymentStatus: (status: BookingPaymentStatus) => void;
  clearBooking: () => void;
}

export const useBookingStore = create<BookingStore>((set) => ({
  currentBooking: null,
  
  setCurrentBooking: (booking) => set({ currentBooking: booking }),
  
  updateBookingStatus: (status) => set((state) => ({
    currentBooking: state.currentBooking ? { ...state.currentBooking, status } : null
  })),
  
  updatePaymentStatus: (status) => set((state) => ({
    currentBooking: state.currentBooking ? { ...state.currentBooking, paymentStatus: status } : null
  })),
  
  clearBooking: () => set({ currentBooking: null })
}));
