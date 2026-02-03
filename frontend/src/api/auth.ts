import { ApiClient } from './http';
import type {
  AuthResponseDto,
  LoginRequestDto,
  RefreshTokenRequestDto,
  RegisterRequestDto,
} from './dtos/auth';

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
  async login(request: LoginRequestDto, signal?: AbortSignal): Promise<AuthResponseDto> {
    return this.client.postJson<AuthResponseDto>('/api/auth/login', request, signal);
  }

  /**
   * Registers a new user.
   */
  async register(request: RegisterRequestDto, signal?: AbortSignal): Promise<AuthResponseDto> {
    return this.client.postJson<AuthResponseDto>('/api/auth/register', request, signal);
  }

  /**
   * Refreshes an access token using a refresh token.
   */
  async refreshToken(
    request: RefreshTokenRequestDto,
    signal?: AbortSignal,
  ): Promise<AuthResponseDto> {
    return this.client.postJson<AuthResponseDto>('/api/auth/refresh', request, signal);
  }
}
