import type { IAuthRepository, LoginResult } from '../../domain/interfaces/IAuthRepository';
import type { LoginCredentials } from '../../domain/entities/user';
import apiClient from '../http/apiClient';

interface LoginApiResponse {
  accessToken: string;
  expiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  username: string;
  role: string;
}

class AuthRepository implements IAuthRepository {
  async login(credentials: LoginCredentials): Promise<LoginResult> {
    const { data } = await apiClient.post<LoginApiResponse>('/api/auth/login', credentials);
    return {
      user: { username: data.username, role: data.role },
      tokens: {
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        expiresAtUtc: data.expiresAtUtc,
        refreshTokenExpiresAtUtc: data.refreshTokenExpiresAtUtc,
      },
    };
  }

  async logout(refreshToken: string): Promise<void> {
    await apiClient.post('/api/auth/revoke', { refreshToken });
  }
}

export const authRepository = new AuthRepository();
