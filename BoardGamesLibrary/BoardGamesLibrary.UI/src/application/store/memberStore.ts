import { create } from 'zustand';
import type { Member, CreateMemberRequest, UpdateMemberRequest } from '../../domain/entities/member';
import type { PagedResult } from '../../domain/interfaces/common';
import { memberRepository } from '../../infrastructure/repositories/memberRepository';

interface MemberState {
  pagedResult: PagedResult<Member> | null;
  isLoading: boolean;
  error: string | null;
  fetchPage: (page: number, pageSize?: number) => Promise<void>;
  create: (data: CreateMemberRequest) => Promise<void>;
  update: (id: number, data: UpdateMemberRequest) => Promise<void>;
  clearError: () => void;
}

export const useMemberStore = create<MemberState>((set, get) => ({
  pagedResult: null,
  isLoading: false,
  error: null,

  fetchPage: async (page, pageSize = 20) => {
    set({ isLoading: true, error: null });
    try {
      const pagedResult = await memberRepository.list(page, pageSize);
      set({ pagedResult, isLoading: false });
    } catch {
      set({ error: 'Failed to load members.', isLoading: false });
    }
  },

  create: async (data) => {
    set({ isLoading: true, error: null });
    try {
      await memberRepository.create(data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to create member.', isLoading: false });
    }
  },

  update: async (id, data) => {
    set({ isLoading: true, error: null });
    try {
      await memberRepository.update(id, data);
      await get().fetchPage(get().pagedResult?.page ?? 1);
    } catch {
      set({ error: 'Failed to update member.', isLoading: false });
    }
  },

  clearError: () => set({ error: null }),
}));
