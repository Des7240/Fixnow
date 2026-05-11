import { create } from 'zustand';
import axiosInstance from '../utils/axiosInstance';

interface OpenJob {
  id: string;
  title: string;
  description: string;
  address: string;
  serviceName: string;
  serviceId: string;
  createdAt: string;
  offerCount: number;
  minBudget?: number;
  maxBudget?: number;
  urgencyLevel?: string;
  distanceKm?: number;
  isSaved?: boolean;
}

interface MarketplaceFilters {
  radius: number;
  serviceIds: string[];
  minBudget?: number;
  maxBudget?: number;
  urgencyLevel?: string;
  sort: string;
}

interface MarketplaceState {
  jobs: OpenJob[];
  loading: boolean;
  filters: MarketplaceFilters;
  setFilters: (filters: Partial<MarketplaceFilters>) => void;
  fetchJobs: (lat: number, lng: number) => Promise<void>;
  saveJob: (jobId: string) => Promise<void>;
  unsaveJob: (jobId: string) => Promise<void>;
}

export const useMarketplaceStore = create<MarketplaceState>((set, get) => ({
  jobs: [],
  loading: false,
  filters: {
    radius: 10,
    serviceIds: [],
    sort: 'latest',
  },
  setFilters: (newFilters) => {
    set((state) => ({
      filters: { ...state.filters, ...newFilters },
    }));
  },
  fetchJobs: async (lat, lng) => {
    set({ loading: true });
    try {
      const { filters } = get();
      const params = new URLSearchParams();
      params.append('lat', lat.toString());
      params.append('lng', lng.toString());
      params.append('radius', filters.radius.toString());
      params.append('sort', filters.sort);
      
      if (filters.serviceIds.length > 0) {
        params.append('serviceTypes', filters.serviceIds.join(','));
      }
      if (filters.minBudget) params.append('minBudget', filters.minBudget.toString());
      if (filters.maxBudget) params.append('maxBudget', filters.maxBudget.toString());
      if (filters.urgencyLevel) params.append('urgencyLevel', filters.urgencyLevel);

      const res = await axiosInstance.get(`/open-jobs/marketplace?${params.toString()}`);
      
      // Also fetch saved jobs to mark them
      const savedRes = await axiosInstance.get('/open-jobs/saved');
      const savedIds = new Set(savedRes.data.map((j: any) => j.id));

      const jobsWithSaved = res.data.map((j: OpenJob) => ({
        ...j,
        isSaved: savedIds.has(j.id)
      }));

      set({ jobs: jobsWithSaved });
    } catch (err) {
      console.error('Failed to fetch marketplace jobs', err);
    } finally {
      set({ loading: false });
    }
  },
  saveJob: async (jobId) => {
    try {
      await axiosInstance.post(`/open-jobs/${jobId}/save`);
      set((state) => ({
        jobs: state.jobs.map((j) => j.id === jobId ? { ...j, isSaved: true } : j)
      }));
    } catch (err) {
      console.error('Failed to save job', err);
    }
  },
  unsaveJob: async (jobId) => {
    try {
      await axiosInstance.delete(`/open-jobs/${jobId}/save`);
      set((state) => ({
        jobs: state.jobs.map((j) => j.id === jobId ? { ...j, isSaved: false } : j)
      }));
    } catch (err) {
      console.error('Failed to unsave job', err);
    }
  }
}));
