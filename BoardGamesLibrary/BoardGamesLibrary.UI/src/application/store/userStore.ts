import { create } from 'zustand';
import type { AppUser, CreateUserRequest, UpdateUserRequest } from '../../domain/entities/appUser';
import type { PagedResult } from '../../domain/interfaces/common';
import { userRepository } from '../../infrastructure/repositories/userRepository';

interface UserState {
  pagedResult: PagedResult<AppUser> | null;
  isLoading: boolean;
  error: string | null;
  fetchPage: (page: number, pageSize?: number) => Promise<void>;
  create: (data: CreateUserRequest) => Promise<void>;
  update: (id: number, data: UpdateUserRequest) => Promise<void>;
  clearError: () => void;
}

export const useUserStore = create<UserState>((set, get) => ({
  pagedResult: null,
  isLoading: false,
  error: null,

  fetchPage: async (page, pageSize = 20) => {
    set({ isLoading: true, error: null });
    try {
      const pagedResult = await userRepository.list(page, pageSize);
      set({ pagedResult, isLoading: false });
    } catch {
      set({ error: 'Failed to load users.', isLoading: false });
    }
  },

  create: async (data) => {
    set({ isLoading: true, error: null });
    try {
      await userRepository.create(data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to create user.', isLoading: false });
    }
  },

  update: async (id, data) => {
    set({ isLoading: true, error: null });
    try {
      await userRepository.update(id, data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to update user.', isLoading: false });
    }
  },

  clearError: () => set({ error: null }),
}));
