import { useShallow } from 'zustand/react/shallow';
import { useBoardGameStore } from '../../application/store/boardGameStore';

export function useBoardGames() {
  return useBoardGameStore(useShallow((s) => ({
    pagedResult: s.pagedResult,
    isLoading: s.isLoading,
    error: s.error,
    fetchPage: s.fetchPage,
    create: s.create,
    update: s.update,
    clearError: s.clearError,
  })));
}
