import { create } from 'zustand';
import type { Inventory, UpdateInventoryRequest } from '../../domain/entities/inventory';
import type { PagedResult } from '../../domain/interfaces/common';
import { inventoryRepository } from '../../infrastructure/repositories/inventoryRepository';

interface InventoryState {
  pagedResult: PagedResult<Inventory> | null;
  isLoading: boolean;
  error: string | null;
  fetchPage: (page: number, pageSize?: number) => Promise<void>;
  update: (boardGameId: number, data: UpdateInventoryRequest) => Promise<void>;
  clearError: () => void;
}

export const useInventoryStore = create<InventoryState>((set, get) => ({
  pagedResult: null,
  isLoading: false,
  error: null,

  fetchPage: async (page, pageSize = 20) => {
    set({ isLoading: true, error: null });
    try {
      const pagedResult = await inventoryRepository.list(page, pageSize);
      set({ pagedResult, isLoading: false });
    } catch {
      set({ error: 'Failed to load inventory.', isLoading: false });
    }
  },

  update: async (boardGameId, data) => {
    set({ isLoading: true, error: null });
    try {
      await inventoryRepository.update(boardGameId, data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to update inventory.', isLoading: false });
    }
  },

  clearError: () => set({ error: null }),
}));
