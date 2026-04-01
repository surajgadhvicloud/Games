import { useShallow } from 'zustand/react/shallow';
import { useAuthStore } from '../../application/store/authStore';

export function useAuth() {
  return useAuthStore(useShallow((state) => ({
    user: state.user,
    isLoading: state.isLoading,
    error: state.error,
    login: state.login,
    logout: state.logout,
    clearError: state.clearError,
  })));
}
