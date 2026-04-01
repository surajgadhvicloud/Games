import type { Member, CreateMemberRequest, UpdateMemberRequest } from '../entities/member';
import type { PagedResult } from '../interfaces/common';

export interface IMemberRepository {
  list(page: number, pageSize: number): Promise<PagedResult<Member>>;
  create(data: CreateMemberRequest): Promise<Member>;
  update(id: number, data: UpdateMemberRequest): Promise<Member>;
}
