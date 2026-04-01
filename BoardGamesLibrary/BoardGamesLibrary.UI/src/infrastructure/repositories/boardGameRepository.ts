import type { IBoardGameRepository } from '../../domain/interfaces/IBoardGameRepository';
import type { BoardGame, CreateBoardGameRequest, UpdateBoardGameRequest } from '../../domain/entities/boardGame';
import type { PagedResult } from '../../domain/interfaces/common';
import apiClient from '../http/apiClient';

class BoardGameRepository implements IBoardGameRepository {
  async list(page: number, pageSize: number): Promise<PagedResult<BoardGame>> {
    const { data } = await apiClient.get<PagedResult<BoardGame>>('/api/boardgames', {
      params: { page, pageSize },
    });
    return data;
  }

  async create(payload: CreateBoardGameRequest): Promise<BoardGame> {
    const { data } = await apiClient.post<BoardGame>('/api/boardgames', payload);
    return data;
  }

  async update(id: number, payload: UpdateBoardGameRequest): Promise<BoardGame> {
    const { data } = await apiClient.put<BoardGame>(`/api/boardgames/${id}`, payload);
    return data;
  }
}

export const boardGameRepository = new BoardGameRepository();
