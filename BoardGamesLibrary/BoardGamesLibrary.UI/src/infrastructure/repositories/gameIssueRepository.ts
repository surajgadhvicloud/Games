import type { IGameIssueRepository } from '../../domain/interfaces/IGameIssueRepository';
import type { GameIssue, CreateGameIssueRequest, UpdateGameIssueRequest } from '../../domain/entities/gameIssue';
import type { PagedResult } from '../../domain/interfaces/common';
import apiClient from '../http/apiClient';

class GameIssueRepository implements IGameIssueRepository {
  async list(page: number, pageSize: number): Promise<PagedResult<GameIssue>> {
    const { data } = await apiClient.get<PagedResult<GameIssue>>('/api/gameissues', {
      params: { page, pageSize },
    });
    return data;
  }

  async create(payload: CreateGameIssueRequest): Promise<GameIssue> {
    const { data } = await apiClient.post<GameIssue>('/api/gameissues', payload);
    return data;
  }

  async update(id: number, payload: UpdateGameIssueRequest): Promise<GameIssue> {
    const { data } = await apiClient.put<GameIssue>(`/api/gameissues/${id}`, payload);
    return data;
  }
}

export const gameIssueRepository = new GameIssueRepository();
