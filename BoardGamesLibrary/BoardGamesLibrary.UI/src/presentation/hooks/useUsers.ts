import { useShallow } from 'zustand/react/shallow';
import { useUserStore } from '../../application/store/userStore';

export function useUsers() {
  return useUserStore(useShallow((s) => ({
    pagedResult: s.pagedResult,
    isLoading: s.isLoading,
    error: s.error,
    fetchPage: s.fetchPage,
    create: s.create,
    update: s.update,
    clearError: s.clearError,
  })));
}
