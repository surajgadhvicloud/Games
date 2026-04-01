export interface BoardGame {
  id: number;
  gameName: string;
  version: string;
  minPlayers: number;
  maxPlayers: number;
  price: number;
  imageUrl: string | null;
}

export interface CreateBoardGameRequest {
  gameName: string;
  version: string;
  minPlayers: number;
  maxPlayers: number;
  price: number;
  imageUrl?: string | null;
}

export interface UpdateBoardGameRequest extends CreateBoardGameRequest {}
