import type { GameIssue, CreateGameIssueRequest, UpdateGameIssueRequest } from '../entities/gameIssue';
import type { PagedResult } from '../interfaces/common';

export interface IGameIssueRepository {
  list(page: number, pageSize: number): Promise<PagedResult<GameIssue>>;
  create(data: CreateGameIssueRequest): Promise<GameIssue>;
  update(id: number, data: UpdateGameIssueRequest): Promise<GameIssue>;
}
