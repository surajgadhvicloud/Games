import type { BoardGame, CreateBoardGameRequest, UpdateBoardGameRequest } from '../entities/boardGame';
import type { PagedResult } from '../interfaces/common';

export interface IBoardGameRepository {
  list(page: number, pageSize: number): Promise<PagedResult<BoardGame>>;
  create(data: CreateBoardGameRequest): Promise<BoardGame>;
  update(id: number, data: UpdateBoardGameRequest): Promise<BoardGame>;
}
