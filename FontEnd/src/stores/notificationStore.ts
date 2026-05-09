import { create } from 'zustand';
import axiosInstance from '../utils/axiosInstance';

interface NotificationState {
  unreadCount: number;
  fetchUnreadCount: () => Promise<void>;
  incrementUnreadCount: () => void;
  decrementUnreadCount: () => void;
  setUnreadCount: (count: number) => void;
}

export const useNotificationStore = create<NotificationState>((set) => ({
  unreadCount: 0,
  fetchUnreadCount: async () => {
    try {
      const res = await axiosInstance.get('/notifications/unread-count');
      set({ unreadCount: res.data });
    } catch (err) {
      console.error('Failed to fetch unread count');
    }
  },
  incrementUnreadCount: () => set((state) => ({ unreadCount: state.unreadCount + 1 })),
  decrementUnreadCount: () => set((state) => ({ unreadCount: Math.max(0, state.unreadCount - 1) })),
  setUnreadCount: (count) => set({ unreadCount: count }),
}));
