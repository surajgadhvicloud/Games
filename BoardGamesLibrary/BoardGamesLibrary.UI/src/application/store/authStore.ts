import { create } from 'zustand';
import type { User } from '../../domain/entities/user';
import { authRepository } from '../../infrastructure/repositories/authRepository';

interface AuthState {
  user: User | null;
  isLoading: boolean;
  error: string | null;

  /** Use-case: sign in */
  login: (username: string, password: string) => Promise<void>;
  /** Use-case: sign out (best-effort token revocation) */
  logout: () => Promise<void>;
  clearError: () => void;
}

function hydrateUserFromStorage(): User | null {
  const username = localStorage.getItem('username');
  const role = localStorage.getItem('role');
  return username && role ? { username, role } : null;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: hydrateUserFromStorage(),
  isLoading: false,
  error: null,

  login: async (username, password) => {
    set({ isLoading: true, error: null });
    try {
      const { user, tokens } = await authRepository.login({ username, password });
      localStorage.setItem('accessToken', tokens.accessToken);
      localStorage.setItem('refreshToken', tokens.refreshToken);
      localStorage.setItem('username', user.username);
      localStorage.setItem('role', user.role);
      set({ user, isLoading: false });
    } catch {
      set({ error: 'Invalid username or password.', isLoading: false });
    }
  },

  logout: async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      try {
        await authRepository.logout(refreshToken);
      } catch {
        // best-effort — clear local state regardless
      }
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    set({ user: null });
  },

  clearError: () => set({ error: null }),
}));
