import { create } from 'zustand';
import axiosInstance from '../utils/axiosInstance';

export interface OpenJob {
  id: string;
  customerId: string;
  customerName: string;
  customerAvatar?: string;
  serviceId: string;
  serviceName: string;
  title: string;
  description: string;
  address: string;
  lat: number;
  lng: number;
  radiusKm: number;
  minBudget?: number;
  maxBudget?: number;
  urgencyLevel?: string;
  status: string;
  createdAt: string;
  offerCount: number;
  fileUrls: string[];
}

export interface Offer {
  id: string;
  openJobId: string;
  workerId: string;
  workerName: string;
  workerAvatar?: string;
  workerRating: number;
  workerCompletedJobs: number;
  workerScore: number;
  estimatedPrice: number;
  analysis: string;
  estimatedArrivalMinutes: number;
  estimatedRepairTimeMinutes: number;
  warrantyDays?: number;
  status: string;
  createdAt: string;
  fileUrls: string[];
}

interface OpenJobStore {
  myJobs: OpenJob[];
  adminJobs: OpenJob[];
  loading: boolean;
  fetchMyJobs: () => Promise<void>;
  fetchAdminJobs: () => Promise<void>;
  closeJob: (jobId: string, reason?: string) => Promise<void>;
  selectWorker: (jobId: string, offerId: string) => Promise<void>;
  rejectOffer: (offerId: string) => Promise<void>;
  moderateJob: (jobId: string, status: string, reason?: string) => Promise<void>;
  deleteJob: (jobId: string) => Promise<void>;
}

export const useOpenJobStore = create<OpenJobStore>((set) => ({
  myJobs: [],
  adminJobs: [],
  loading: false,

  fetchMyJobs: async () => {
    set({ loading: true });
    try {
      const res = await axiosInstance.get('/open-jobs/my-jobs');
      set({ myJobs: res.data });
    } catch (err) {
      console.error('Failed to fetch my jobs', err);
    } finally {
      set({ loading: false });
    }
  },

  fetchAdminJobs: async () => {
    set({ loading: true });
    try {
      const res = await axiosInstance.get('/admin/open-jobs');
      set({ adminJobs: res.data });
    } catch (err) {
      console.error('Failed to fetch admin jobs', err);
    } finally {
      set({ loading: false });
    }
  },

  closeJob: async (jobId, reason) => {
    try {
      await axiosInstance.post(`/open-jobs/${jobId}/close`, { reason });
      set((state) => ({
        myJobs: state.myJobs.map((j) => j.id === jobId ? { ...j, status: 'CLOSED' } : j)
      }));
    } catch (err) {
      console.error('Failed to close job', err);
      throw err;
    }
  },

  selectWorker: async (jobId, offerId) => {
    try {
      await axiosInstance.post(`/open-jobs/${jobId}/select-worker`, { offerId });
      set((state) => ({
        myJobs: state.myJobs.map((j) => j.id === jobId ? { ...j, status: 'BOOKING_CREATED' } : j)
      }));
    } catch (err) {
      console.error('Failed to select worker', err);
      throw err;
    }
  },

  rejectOffer: async (offerId) => {
    try {
      await axiosInstance.post(`/open-jobs/offers/${offerId}/reject`);
    } catch (err) {
      console.error('Failed to reject offer', err);
      throw err;
    }
  },

  moderateJob: async (jobId, status, reason) => {
    try {
      await axiosInstance.post(`/admin/open-jobs/${jobId}/moderate`, { status, reason });
      set((state) => ({
        adminJobs: state.adminJobs.map((j) => 
            j.id === jobId ? { ...j, moderationStatus: status, status: (status === 'REMOVED' || status === 'BANNED' ? 'CLOSED' : j.status) } : j
        )
      }));
    } catch (err) {
      console.error('Failed to moderate job', err);
      throw err;
    }
  },

  deleteJob: async (jobId) => {
    try {
      await axiosInstance.delete(`/admin/open-jobs/${jobId}`);
      set((state) => ({
        adminJobs: state.adminJobs.filter((j) => j.id !== jobId)
      }));
    } catch (err) {
      console.error('Failed to delete job', err);
      throw err;
    }
  }
}));
