import type { IUserRepository } from '../../domain/interfaces/IUserRepository';
import type { AppUser, CreateUserRequest, UpdateUserRequest } from '../../domain/entities/appUser';
import type { PagedResult } from '../../domain/interfaces/common';
import apiClient from '../http/apiClient';

class UserRepository implements IUserRepository {
  async list(page: number, pageSize: number): Promise<PagedResult<AppUser>> {
    const { data } = await apiClient.get<PagedResult<AppUser>>('/api/users', {
      params: { page, pageSize },
    });
    return data;
  }

  async create(payload: CreateUserRequest): Promise<AppUser> {
    const { data } = await apiClient.post<AppUser>('/api/users', payload);
    return data;
  }

  async update(id: number, payload: UpdateUserRequest): Promise<AppUser> {
    const { data } = await apiClient.put<AppUser>(`/api/users/${id}`, payload);
    return data;
  }
}

export const userRepository = new UserRepository();
