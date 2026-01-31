export type ApiError = {
  status: number;
  message: string;
  details?: unknown;
};

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

  constructor(baseUrl?: string) {
    const envUrl = import.meta.env.VITE_API_BASE_URL as string | undefined;
    this.baseUrl = (baseUrl ?? envUrl ?? '').trim();
    if (!this.baseUrl) {
      throw new Error('Missing VITE_API_BASE_URL (set it in .env.local).');
    }
  }

  async request<T>(
    path: string,
    options?: {
      method?: string;
      headers?: Record<string, string>;
      body?: BodyInit | null;
      signal?: AbortSignal;
    },
  ): Promise<T> {
    const response = await fetch(joinUrl(this.baseUrl, path), {
      method: options?.method ?? 'GET',
      headers: options?.headers,
      body: options?.body ?? null,
      signal: options?.signal,
    });

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

  get<T>(path: string, signal?: AbortSignal): Promise<T> {
    return this.request<T>(path, { method: 'GET', signal });
  }

  postJson<T>(path: string, body: unknown, signal?: AbortSignal): Promise<T> {
    return this.request<T>(path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
      signal,
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
