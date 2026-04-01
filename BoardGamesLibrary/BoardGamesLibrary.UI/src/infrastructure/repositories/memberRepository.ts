import type { IMemberRepository } from '../../domain/interfaces/IMemberRepository';
import type { Member, CreateMemberRequest, UpdateMemberRequest } from '../../domain/entities/member';
import type { PagedResult } from '../../domain/interfaces/common';
import apiClient from '../http/apiClient';

class MemberRepository implements IMemberRepository {
  async list(page: number, pageSize: number): Promise<PagedResult<Member>> {
    const { data } = await apiClient.get<PagedResult<Member>>('/api/members', {
      params: { page, pageSize },
    });
    return data;
  }

  async create(payload: CreateMemberRequest): Promise<Member> {
    const { data } = await apiClient.post<Member>('/api/members', payload);
    return data;
  }

  async update(id: number, payload: UpdateMemberRequest): Promise<Member> {
    const { data } = await apiClient.put<Member>(`/api/members/${id}`, payload);
    return data;
  }
}

export const memberRepository = new MemberRepository();
