import type { Inventory, UpdateInventoryRequest } from '../entities/inventory';
import type { PagedResult } from '../interfaces/common';

export interface IInventoryRepository {
  list(page: number, pageSize: number): Promise<PagedResult<Inventory>>;
  update(boardGameId: number, data: UpdateInventoryRequest): Promise<Inventory>;
}
