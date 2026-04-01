import type { GameCondition, GameIssueStatus } from '../enums';

export interface GameIssue {
  id: number;
  boardGameId: number;
  userId: number;
  photoUrlBeforeIssue: string | null;
  photoUrlAfterReturn: string | null;
  startDateUtc: string;
  endDateUtc: string;
  returnDateUtc: string | null;
  conditionGivenOut: GameCondition;
  conditionGivenIn: GameCondition | null;
  overdueCharges: number;
  status: GameIssueStatus;
}

export interface CreateGameIssueRequest {
  boardGameId: number;
  userId: number;
  startDateUtc?: string | null;
  endDateUtc?: string | null;
  conditionGivenOut: GameCondition;
  photoUrlBeforeIssue?: string | null;
}

export interface UpdateGameIssueRequest {
  returnDateUtc?: string | null;
  conditionGivenIn?: GameCondition | null;
  photoUrlAfterReturn?: string | null;
}
