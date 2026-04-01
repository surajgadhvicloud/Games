import { useShallow } from 'zustand/react/shallow';
import { useGameIssueStore } from '../../application/store/gameIssueStore';

export function useGameIssues() {
  return useGameIssueStore(useShallow((s) => ({
    pagedResult: s.pagedResult,
    isLoading: s.isLoading,
    error: s.error,
    fetchPage: s.fetchPage,
    create: s.create,
    update: s.update,
    clearError: s.clearError,
  })));
}
