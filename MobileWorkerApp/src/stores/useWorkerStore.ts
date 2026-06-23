import { create } from 'zustand';

interface WorkerProfile {
  userId: string;
  bio?: string;
  experienceYears: number;
  averageRating: number;
  totalJobs: number;
  availabilityStatus: string;
}

interface Wallet {
  id: string;
  balance: number;
  currency: string;
}

interface WorkerState {
  profile: WorkerProfile | null;
  wallet: Wallet | null;
  setProfile: (profile: WorkerProfile | null) => void;
  setWallet: (wallet: Wallet | null) => void;
}

export const useWorkerStore = create<WorkerState>((set) => ({
  profile: null,
  wallet: null,
  setProfile: (profile) => set({ profile }),
  setWallet: (wallet) => set({ wallet }),
}));
