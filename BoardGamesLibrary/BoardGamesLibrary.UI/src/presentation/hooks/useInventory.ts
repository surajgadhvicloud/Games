import { useShallow } from 'zustand/react/shallow';
import { useInventoryStore } from '../../application/store/inventoryStore';

export function useInventory() {
  return useInventoryStore(useShallow((s) => ({
    pagedResult: s.pagedResult,
    isLoading: s.isLoading,
    error: s.error,
    fetchPage: s.fetchPage,
    update: s.update,
    clearError: s.clearError,
  })));
}
