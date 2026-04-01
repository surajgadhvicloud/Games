export interface Inventory {
  id: number;
  boardGameId: number;
  isMissingOrBroken: boolean;
  totalInventory: number;
  availableInventory: number;
}

export interface UpdateInventoryRequest {
  isMissingOrBroken: boolean;
  totalInventory: number;
  availableInventory: number;
}
