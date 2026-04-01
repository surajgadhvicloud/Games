import type { AppUser, CreateUserRequest, UpdateUserRequest } from '../entities/appUser';
import type { PagedResult } from '../interfaces/common';

export interface IUserRepository {
  list(page: number, pageSize: number): Promise<PagedResult<AppUser>>;
  create(data: CreateUserRequest): Promise<AppUser>;
  update(id: number, data: UpdateUserRequest): Promise<AppUser>;
}
