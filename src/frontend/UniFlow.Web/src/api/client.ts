import { apiUrl, REQUEST_TIMEOUT_MS } from '../config';
import type { ApiResult } from './types';

const TOKEN_KEY = 'uniflow_access_token';

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setStoredToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearStoredToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

function mapHttpStatusError<T>(status: number): ApiResult<T> | null {
  if (status === 401) {
    return {
      isSuccess: false,
      error: { code: 'UNAUTHORIZED', message: 'Oturum süresi doldu. Lütfen tekrar giriş yapın.' },
    };
  }

  if (status === 429) {
    return {
      isSuccess: false,
      error: { code: 'RATE_LIMIT', message: 'Çok fazla istek. Kısa süre sonra tekrar deneyin.' },
    };
  }

  if (status >= 500) {
    return {
      isSuccess: false,
      error: { code: 'SERVER_ERROR', message: 'Sunucu hatası. Lütfen daha sonra tekrar deneyin.' },
    };
  }

  return null;
}

function mapFetchError<T>(error: unknown): ApiResult<T> {
  if (error instanceof DOMException && error.name === 'AbortError') {
    return {
      isSuccess: false,
      error: { code: 'TIMEOUT', message: 'Sunucu süre içinde yanıt vermedi.' },
    };
  }

  if (error instanceof TypeError) {
    return {
      isSuccess: false,
      error: {
        code: 'NETWORK',
        message: 'Sunucuya bağlanılamıyor. API adresini ve bağlantınızı kontrol edin.',
      },
    };
  }

  return {
    isSuccess: false,
    error: { code: 'CLIENT', message: 'Beklenmeyen bir hata oluştu.' },
  };
}

async function parseBody<T>(response: Response): Promise<ApiResult<T>> {
  const statusError = mapHttpStatusError<T>(response.status);
  if (statusError) {
    return statusError;
  }

  const text = await response.text();
  if (!text.trim()) {
    return {
      isSuccess: false,
      error: { code: 'PARSE', message: 'Sunucu yanıtı okunamadı.' },
    };
  }

  try {
    const parsed = JSON.parse(text) as ApiResult<T>;
    if (typeof parsed.isSuccess === 'boolean') {
      return parsed;
    }
  } catch {
    // fall through
  }

  return {
    isSuccess: false,
    error: { code: 'PARSE', message: 'Sunucu yanıtı beklenen formatta değil.' },
  };
}

type RequestOptions = {
  method?: string;
  body?: BodyInit | null;
  auth?: boolean;
  headers?: Record<string, string>;
};

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<ApiResult<T>> {
  const controller = new AbortController();
  const timeoutId = window.setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  const headers: Record<string, string> = { ...options.headers };

  if (options.auth !== false) {
    const token = getStoredToken();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
  }

  if (options.body && !(options.body instanceof FormData)) {
    headers['Content-Type'] = 'application/json';
  }

  try {
    const response = await fetch(apiUrl(path), {
      method: options.method ?? 'GET',
      headers,
      body: options.body ?? null,
      signal: controller.signal,
    });

    return await parseBody<T>(response);
  } catch (error) {
    return mapFetchError<T>(error);
  } finally {
    window.clearTimeout(timeoutId);
  }
}

export function getErrorMessage(result: ApiResult<unknown>, fallback = 'İşlem başarısız.'): string {
  return result.error?.message?.trim() || fallback;
}
