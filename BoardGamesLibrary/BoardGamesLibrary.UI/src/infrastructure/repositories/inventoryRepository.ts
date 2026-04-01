import type { IInventoryRepository } from '../../domain/interfaces/IInventoryRepository';
import type { Inventory, UpdateInventoryRequest } from '../../domain/entities/inventory';
import type { PagedResult } from '../../domain/interfaces/common';
import apiClient from '../http/apiClient';

class InventoryRepository implements IInventoryRepository {
  async list(page: number, pageSize: number): Promise<PagedResult<Inventory>> {
    const { data } = await apiClient.get<PagedResult<Inventory>>('/api/inventories', {
      params: { page, pageSize },
    });
    return data;
  }

  async update(boardGameId: number, payload: UpdateInventoryRequest): Promise<Inventory> {
    const { data } = await apiClient.put<Inventory>(`/api/inventories/${boardGameId}`, payload);
    return data;
  }
}

export const inventoryRepository = new InventoryRepository();
