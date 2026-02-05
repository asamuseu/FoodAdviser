import { ApiClient } from './http';
import type {
  AuthResponseModel,
  LoginRequestModel,
  RefreshTokenRequestModel,
  RegisterRequestModel,
} from './models/auth';

/**
 * API client for authentication endpoints.
 */
export class AuthApi {
  private readonly client: ApiClient;

  constructor(client?: ApiClient) {
    this.client = client ?? new ApiClient();
  }

  /**
   * Authenticates a user and returns JWT tokens.
   */
  async login(request: LoginRequestModel, signal?: AbortSignal): Promise<AuthResponseModel> {
    return this.client.postJson<AuthResponseModel>('/api/auth/login', request, signal);
  }

  /**
   * Registers a new user.
   */
  async register(request: RegisterRequestModel, signal?: AbortSignal): Promise<AuthResponseModel> {
    return this.client.postJson<AuthResponseModel>('/api/auth/register', request, signal);
  }

  /**
   * Refreshes an access token using a refresh token.
   * Note: This endpoint skips authentication to prevent infinite refresh loops.
   */
  async refreshToken(
    request: RefreshTokenRequestModel,
    signal?: AbortSignal,
  ): Promise<AuthResponseModel> {
    return this.client.postJson<AuthResponseModel>(
      '/api/auth/refresh',
      request,
      signal,
      { skipAuth: true },
    );
  }
}
