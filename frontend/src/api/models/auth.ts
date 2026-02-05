/**
 * Request model for user login.
 */
export interface LoginRequestModel {
  email: string;
  password: string;
}

/**
 * Request model for user registration.
 */
export interface RegisterRequestModel {
  email: string;
  password: string;
  confirmPassword: string;
  firstName?: string | null;
  lastName?: string | null;
}

/**
 * Request model for refreshing tokens.
 */
export interface RefreshTokenRequestModel {
  refreshToken: string;
}

/**
 * Response model for successful authentication.
 */
export interface AuthResponseModel {
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
