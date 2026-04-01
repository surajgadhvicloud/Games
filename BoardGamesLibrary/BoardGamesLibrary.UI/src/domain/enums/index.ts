export const UserType = {
  Regular: 0,
  Premium: 1,
} as const;
export type UserType = (typeof UserType)[keyof typeof UserType];

export const UserRole = {
  Admin: 1,
  DataEntry: 2,
  Manager: 3,
} as const;
export type UserRole = (typeof UserRole)[keyof typeof UserRole];

export const GameCondition = {
  Mint: 0,
  Lost: 1,
  Broken: 2,
  CompleteNotMint: 3,
} as const;
export type GameCondition = (typeof GameCondition)[keyof typeof GameCondition];

export const GameIssueStatus = {
  Active: 0,
  Returned: 1,
  Overdue: 2,
} as const;
export type GameIssueStatus = (typeof GameIssueStatus)[keyof typeof GameIssueStatus];

/** Helper: get enum key name from value */
export function enumLabel<T extends Record<string, number>>(obj: T, value: number): string {
  return (Object.entries(obj).find(([, v]) => v === value)?.[0]) ?? String(value);
}
