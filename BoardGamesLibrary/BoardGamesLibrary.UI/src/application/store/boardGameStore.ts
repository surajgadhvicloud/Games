import { create } from 'zustand';
import type { BoardGame, CreateBoardGameRequest, UpdateBoardGameRequest } from '../../domain/entities/boardGame';
import type { PagedResult } from '../../domain/interfaces/common';
import { boardGameRepository } from '../../infrastructure/repositories/boardGameRepository';

interface BoardGameState {
  pagedResult: PagedResult<BoardGame> | null;
  isLoading: boolean;
  error: string | null;
  fetchPage: (page: number, pageSize?: number) => Promise<void>;
  create: (data: CreateBoardGameRequest) => Promise<void>;
  update: (id: number, data: UpdateBoardGameRequest) => Promise<void>;
  clearError: () => void;
}

export const useBoardGameStore = create<BoardGameState>((set, get) => ({
  pagedResult: null,
  isLoading: false,
  error: null,

  fetchPage: async (page, pageSize = 20) => {
    set({ isLoading: true, error: null });
    try {
      const pagedResult = await boardGameRepository.list(page, pageSize);
      set({ pagedResult, isLoading: false });
    } catch {
      set({ error: 'Failed to load board games.', isLoading: false });
    }
  },

  create: async (data) => {
    set({ isLoading: true, error: null });
    try {
      await boardGameRepository.create(data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to create board game.', isLoading: false });
    }
  },

  update: async (id, data) => {
    set({ isLoading: true, error: null });
    try {
      await boardGameRepository.update(id, data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to update board game.', isLoading: false });
    }
  },

  clearError: () => set({ error: null }),
}));
