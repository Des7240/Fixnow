import { create } from 'zustand';
import { persist } from 'zustand/middleware';

type Role = 'CUSTOMER' | 'WORKER' | 'ADMIN';

interface User {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  needsPasswordReset?: boolean;
  role: Role;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  setAuth: (user: User, token: string) => void;
  setAccessToken: (token: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      isAuthenticated: false,

      setAuth: (user, token) => set({ user, accessToken: token, isAuthenticated: true }),
      setAccessToken: (token) => set({ accessToken: token }),
      logout: () => set({ user: null, accessToken: null, isAuthenticated: false }),
    }),
    {
      name: 'auth-storage',
      // only store the role/user info. The actual tokens can be here, but RefreshToken is in httpOnly cookie
    }
  )
);
