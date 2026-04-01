import { useShallow } from 'zustand/react/shallow';
import { useMemberStore } from '../../application/store/memberStore';

export function useMembers() {
  return useMemberStore(useShallow((s) => ({
    pagedResult: s.pagedResult,
    isLoading: s.isLoading,
    error: s.error,
    fetchPage: s.fetchPage,
    create: s.create,
    update: s.update,
    clearError: s.clearError,
  })));
}
