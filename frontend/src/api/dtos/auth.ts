/**
 * Request DTO for user login.
 */
export interface LoginRequestDto {
  email: string;
  password: string;
}

/**
 * Request DTO for user registration.
 */
export interface RegisterRequestDto {
  email: string;
  password: string;
  confirmPassword: string;
  firstName?: string | null;
  lastName?: string | null;
}

/**
 * Request DTO for refreshing tokens.
 */
export interface RefreshTokenRequestDto {
  refreshToken: string;
}

/**
 * Response DTO for successful authentication.
 */
export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
}

/**
 * User information extracted from auth response.
 */
export interface User {
  id: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
}
