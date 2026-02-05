import { TokenRefreshManager } from './tokenRefreshManager';

export type ApiError = {
  status: number;
  message: string;
  details?: unknown;
};

/** Storage key for the access token - must match AuthContext */
const ACCESS_TOKEN_KEY = 'foodadviser_access_token';

/**
 * Gets the stored access token from session storage.
 */
function getAccessToken(): string | null {
  return sessionStorage.getItem(ACCESS_TOKEN_KEY);
}

/**
 * Callback type for token refresh operations.
 * Should return the new access token or null if refresh fails.
 */
export type TokenRefreshCallback = () => Promise<string | null>;

/**
 * Callback type for handling logout when token refresh fails.
 */
export type LogoutCallback = () => void;

function joinUrl(baseUrl: string, path: string): string {
  const trimmedBase = baseUrl.replace(/\/+$/, '');
  const trimmedPath = path.replace(/^\/+/, '');
  return `${trimmedBase}/${trimmedPath}`;
}

async function readBodySafe(response: Response): Promise<unknown> {
  const contentType = response.headers.get('content-type') ?? '';
  if (contentType.includes('application/json') || contentType.includes('text/json')) {
    try {
      return await response.json();
    } catch {
      return null;
    }
  }
  try {
    return await response.text();
  } catch {
    return null;
  }
}

export class ApiClient {
  private readonly baseUrl: string;
  private tokenRefreshCallback: TokenRefreshCallback | null = null;
  private logoutCallback: LogoutCallback | null = null;
  private readonly refreshManager = TokenRefreshManager.getInstance();

  constructor(baseUrl?: string) {
    const envUrl = import.meta.env.VITE_API_BASE_URL as string | undefined;
    this.baseUrl = (baseUrl ?? envUrl ?? '').trim();
    if (!this.baseUrl) {
      throw new Error('Missing VITE_API_BASE_URL (set it in .env.local).');
    }
  }

  /**
   * Sets the callback to refresh the access token when it expires.
   * This is called automatically when a request fails with 401.
   */
  setTokenRefreshCallback(callback: TokenRefreshCallback | null): void {
    this.tokenRefreshCallback = callback;
  }

  /**
   * Sets the callback to log out the user when token refresh fails.
   */
  setLogoutCallback(callback: LogoutCallback | null): void {
    this.logoutCallback = callback;
  }

  async request<T>(
    path: string,
    options?: {
      method?: string;
      headers?: Record<string, string>;
      body?: BodyInit | null;
      signal?: AbortSignal;
      skipAuth?: boolean;
      skipRetry?: boolean; // Internal flag to prevent infinite retry loops
    },
  ): Promise<T> {
    // Build headers with optional Authorization
    const headers: Record<string, string> = { ...options?.headers };

    // Add Authorization header if token exists and skipAuth is not set
    if (!options?.skipAuth) {
      const token = getAccessToken();
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    const response = await fetch(joinUrl(this.baseUrl, path), {
      method: options?.method ?? 'GET',
      headers,
      body: options?.body ?? null,
      signal: options?.signal,
    });

    // Handle 401 Unauthorized - attempt token refresh and retry
    if (response.status === 401 && !options?.skipAuth && !options?.skipRetry) {
      const newToken = await this.handleUnauthorized();
      if (newToken) {
        // Retry the request with the new token
        const retryHeaders = { ...headers, Authorization: `Bearer ${newToken}` };
        return this.request<T>(path, {
          ...options,
          headers: retryHeaders,
          skipRetry: true, // Prevent infinite loops
        });
      }
    }

    if (!response.ok) {
      const details = await readBodySafe(response);
      const message =
        typeof details === 'string' && details.trim().length > 0
          ? details
          : response.statusText || `Request failed (${response.status})`;
      const error: ApiError = { status: response.status, message, details };
      throw Object.assign(new Error(message), { apiError: error });
    }

    // Most endpoints return JSON, but some are unspecified in the OpenAPI.
    const contentType = response.headers.get('content-type') ?? '';
    if (contentType.includes('application/json') || contentType.includes('text/json')) {
      return (await response.json()) as T;
    }

    return (await response.text()) as T;
  }

  /**
   * Handles 401 Unauthorized responses by attempting to refresh the access token.
   * Returns the new token if successful, null otherwise.
   */
  private async handleUnauthorized(): Promise<string | null> {
    if (!this.tokenRefreshCallback) {
      // No refresh callback configured - trigger logout if available
      this.logoutCallback?.();
      return null;
    }

    try {
      // Use the refresh manager to ensure only one refresh happens at a time
      const newToken = await this.refreshManager.executeRefresh(this.tokenRefreshCallback);
      
      if (!newToken) {
        // Refresh failed - trigger logout
        this.logoutCallback?.();
      }
      
      return newToken;
    } catch (error) {
      // Refresh failed - trigger logout
      this.logoutCallback?.();
      return null;
    }
  }

  get<T>(path: string, signal?: AbortSignal): Promise<T> {
    return this.request<T>(path, { method: 'GET', signal });
  }

  postJson<T>(
    path: string,
    body: unknown,
    signal?: AbortSignal,
    options?: { skipAuth?: boolean },
  ): Promise<T> {
    return this.request<T>(path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
      signal,
      skipAuth: options?.skipAuth,
    });
  }

  putJson<T>(path: string, body: unknown, signal?: AbortSignal): Promise<T> {
    return this.request<T>(path, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
      signal,
    });
  }

  delete<T>(path: string, signal?: AbortSignal): Promise<T> {
    return this.request<T>(path, { method: 'DELETE', signal });
  }

  postForm<T>(path: string, form: FormData, signal?: AbortSignal): Promise<T> {
    return this.request<T>(path, {
      method: 'POST',
      body: form,
      signal,
    });
  }
}
