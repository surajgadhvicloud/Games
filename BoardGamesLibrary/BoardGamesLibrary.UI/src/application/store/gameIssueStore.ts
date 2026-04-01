import { create } from 'zustand';
import type { GameIssue, CreateGameIssueRequest, UpdateGameIssueRequest } from '../../domain/entities/gameIssue';
import type { PagedResult } from '../../domain/interfaces/common';
import { gameIssueRepository } from '../../infrastructure/repositories/gameIssueRepository';

interface GameIssueState {
  pagedResult: PagedResult<GameIssue> | null;
  isLoading: boolean;
  error: string | null;
  fetchPage: (page: number, pageSize?: number) => Promise<void>;
  create: (data: CreateGameIssueRequest) => Promise<void>;
  update: (id: number, data: UpdateGameIssueRequest) => Promise<void>;
  clearError: () => void;
}

export const useGameIssueStore = create<GameIssueState>((set, get) => ({
  pagedResult: null,
  isLoading: false,
  error: null,

  fetchPage: async (page, pageSize = 20) => {
    set({ isLoading: true, error: null });
    try {
      const pagedResult = await gameIssueRepository.list(page, pageSize);
      set({ pagedResult, isLoading: false });
    } catch {
      set({ error: 'Failed to load game issues.', isLoading: false });
    }
  },

  create: async (data) => {
    set({ isLoading: true, error: null });
    try {
      await gameIssueRepository.create(data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to create game issue.', isLoading: false });
    }
  },

  update: async (id, data) => {
    set({ isLoading: true, error: null });
    try {
      await gameIssueRepository.update(id, data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to update game issue.', isLoading: false });
    }
  },

  clearError: () => set({ error: null }),
}));
