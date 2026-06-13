const rawBase = (import.meta.env.VITE_API_BASE_URL ?? '').trim();

/** Empty in production (same-origin via nginx). Set to http://localhost:5000 in dev. */
export const API_BASE_URL = rawBase.endsWith('/') ? rawBase.slice(0, -1) : rawBase;

export const REQUEST_TIMEOUT_MS = 30_000;

export function apiUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return API_BASE_URL ? `${API_BASE_URL}${normalized}` : normalized;
}
