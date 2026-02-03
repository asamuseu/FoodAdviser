import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { AuthApi } from '../api/auth';
import type {
  AuthResponseDto,
  LoginRequestDto,
  RegisterRequestDto,
  User,
} from '../api/dtos/auth';

const ACCESS_TOKEN_KEY = 'foodadviser_access_token';
const REFRESH_TOKEN_KEY = 'foodadviser_refresh_token';
const TOKEN_EXPIRY_KEY = 'foodadviser_token_expiry';
const USER_KEY = 'foodadviser_user';

interface AuthContextValue {
  /** The current authenticated user, or null if not logged in. */
  user: User | null;
  /** Whether authentication state is still being loaded. */
  isLoading: boolean;
  /** Whether the user is authenticated. */
  isAuthenticated: boolean;
  /** The current access token, or null if not logged in. */
  accessToken: string | null;
  /** Logs in with email and password. Returns the user on success. */
  login: (email: string, password: string) => Promise<User>;
  /** Registers a new user. Returns the user on success. */
  register: (request: RegisterRequestDto) => Promise<User>;
  /** Logs out the current user. */
  logout: () => void;
  /** Refreshes the access token using the stored refresh token. */
  refreshAccessToken: () => Promise<string | null>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

/**
 * Securely stores authentication data.
 * Uses sessionStorage by default for better security (cleared when browser closes).
 * For "remember me" functionality, localStorage could be used instead.
 */
function setAuthData(response: AuthResponseDto): void {
  sessionStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
  sessionStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
  sessionStorage.setItem(TOKEN_EXPIRY_KEY, response.expiresAt);
  sessionStorage.setItem(
    USER_KEY,
    JSON.stringify({
      id: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
    }),
  );
}

function clearAuthData(): void {
  sessionStorage.removeItem(ACCESS_TOKEN_KEY);
  sessionStorage.removeItem(REFRESH_TOKEN_KEY);
  sessionStorage.removeItem(TOKEN_EXPIRY_KEY);
  sessionStorage.removeItem(USER_KEY);
}

function getStoredAccessToken(): string | null {
  return sessionStorage.getItem(ACCESS_TOKEN_KEY);
}

function getStoredRefreshToken(): string | null {
  return sessionStorage.getItem(REFRESH_TOKEN_KEY);
}

function getStoredUser(): User | null {
  const userJson = sessionStorage.getItem(USER_KEY);
  if (!userJson) return null;
  try {
    return JSON.parse(userJson) as User;
  } catch {
    return null;
  }
}

function getStoredTokenExpiry(): Date | null {
  const expiry = sessionStorage.getItem(TOKEN_EXPIRY_KEY);
  if (!expiry) return null;
  const date = new Date(expiry);
  return isNaN(date.getTime()) ? null : date;
}

function isTokenExpired(): boolean {
  const expiry = getStoredTokenExpiry();
  if (!expiry) return true;
  // Consider token expired 30 seconds before actual expiry to account for network latency
  return new Date() >= new Date(expiry.getTime() - 30000);
}

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const authApi = useMemo(() => new AuthApi(), []);

  // Initialize auth state from storage on mount
  useEffect(() => {
    const storedUser = getStoredUser();
    const storedToken = getStoredAccessToken();

    if (storedUser && storedToken && !isTokenExpired()) {
      setUser(storedUser);
      setAccessToken(storedToken);
    } else if (storedUser && getStoredRefreshToken()) {
      // Token expired but we have a refresh token - try to refresh
      // For now, just clear and require re-login
      // A more sophisticated implementation would attempt refresh here
      clearAuthData();
    }

    setIsLoading(false);
  }, []);

  const handleAuthResponse = useCallback((response: AuthResponseDto): User => {
    setAuthData(response);
    const newUser: User = {
      id: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
    };
    setUser(newUser);
    setAccessToken(response.accessToken);
    return newUser;
  }, []);

  const login = useCallback(
    async (email: string, password: string): Promise<User> => {
      const request: LoginRequestDto = { email, password };
      const response = await authApi.login(request);
      return handleAuthResponse(response);
    },
    [authApi, handleAuthResponse],
  );

  const register = useCallback(
    async (request: RegisterRequestDto): Promise<User> => {
      const response = await authApi.register(request);
      return handleAuthResponse(response);
    },
    [authApi, handleAuthResponse],
  );

  const logout = useCallback(() => {
    clearAuthData();
    setUser(null);
    setAccessToken(null);
  }, []);

  const refreshAccessToken = useCallback(async (): Promise<string | null> => {
    const refreshToken = getStoredRefreshToken();
    if (!refreshToken) {
      logout();
      return null;
    }

    try {
      const response = await authApi.refreshToken({ refreshToken });
      handleAuthResponse(response);
      return response.accessToken;
    } catch {
      // Refresh failed - clear auth data and require re-login
      logout();
      return null;
    }
  }, [authApi, handleAuthResponse, logout]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isLoading,
      isAuthenticated: !!user && !!accessToken,
      accessToken,
      login,
      register,
      logout,
      refreshAccessToken,
    }),
    [user, isLoading, accessToken, login, register, logout, refreshAccessToken],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

/**
 * Hook to access the authentication context.
 * Must be used within an AuthProvider.
 */
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

/**
 * Returns the current access token, automatically refreshing if expired.
 * Useful for making authenticated API calls.
 */
export function useAccessToken(): () => Promise<string | null> {
  const { accessToken, refreshAccessToken } = useAuth();

  return useCallback(async () => {
    if (!accessToken) return null;
    if (isTokenExpired()) {
      return refreshAccessToken();
    }
    return accessToken;
  }, [accessToken, refreshAccessToken]);
}
