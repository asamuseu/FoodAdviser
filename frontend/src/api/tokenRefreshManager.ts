/**
 * Manages token refresh operations to prevent concurrent refresh calls.
 * This ensures that when multiple API requests fail with 401 simultaneously,
 * only one token refresh operation is triggered.
 */
export class TokenRefreshManager {
  private static instance: TokenRefreshManager | null = null;
  private refreshPromise: Promise<string | null> | null = null;

  private constructor() {}

  /**
   * Gets the singleton instance of TokenRefreshManager.
   */
  static getInstance(): TokenRefreshManager {
    if (!TokenRefreshManager.instance) {
      TokenRefreshManager.instance = new TokenRefreshManager();
    }
    return TokenRefreshManager.instance;
  }

  /**
   * Executes a token refresh operation, ensuring only one refresh happens at a time.
   * If a refresh is already in progress, returns the existing promise.
   *
   * @param refreshFn - The function that performs the actual token refresh
   * @returns A promise that resolves to the new access token or null if refresh fails
   */
  async executeRefresh(refreshFn: () => Promise<string | null>): Promise<string | null> {
    // If a refresh is already in progress, wait for it
    if (this.refreshPromise) {
      return this.refreshPromise;
    }

    // Start a new refresh
    this.refreshPromise = refreshFn()
      .then((token) => {
        this.refreshPromise = null;
        return token;
      })
      .catch((error) => {
        this.refreshPromise = null;
        throw error;
      });

    return this.refreshPromise;
  }

  /**
   * Clears the current refresh promise.
   * Useful for testing or when explicitly canceling an ongoing refresh.
   */
  clear(): void {
    this.refreshPromise = null;
  }
}
