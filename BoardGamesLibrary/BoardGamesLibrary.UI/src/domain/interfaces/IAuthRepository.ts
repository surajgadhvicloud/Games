import type { LoginCredentials, User, AuthTokens } from '../entities/user';

export interface LoginResult {
  user: User;
  tokens: AuthTokens;
}

/** Port — implemented by infrastructure, consumed by the application store. */
export interface IAuthRepository {
  login(credentials: LoginCredentials): Promise<LoginResult>;
  logout(refreshToken: string): Promise<void>;
}
