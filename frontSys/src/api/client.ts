import type { ApiError, PaginatedResponse } from './types';
import { normalizeToken } from './authTokenStore';

const REMOTE_API =
  (import.meta.env.VITE_API_URL as string | undefined) ??
  'https://appapi.seasbroker.com';

/** Always hit the configured API host (production by default), including local dev */
export const API_BASE = REMOTE_API;

export function resolveApiOrigin(): string {
  if (API_BASE) return API_BASE;
  if (typeof window !== 'undefined') return window.location.origin;
  return REMOTE_API;
}

export class SeasBrokerApiError extends Error {
  status: number;
  data: Record<string, unknown>;

  constructor(message: string, status: number, data: Record<string, unknown> = {}) {
    super(message);
    this.name = 'SeasBrokerApiError';
    this.status = status;
    this.data = data;
  }
}

export interface ApiRequestOptions extends Omit<RequestInit, 'body'> {
  token?: string;
  body?: unknown;
  query?: Record<string, string | number | undefined>;
  /** PocketBase-compatible APIs use raw token; user auth endpoints use Bearer */
  authStyle?: 'raw' | 'bearer';
}

function buildUrl(path: string, query?: Record<string, string | number | undefined>): string {
  const href = path.startsWith('http') ? path : `${API_BASE}${path}`;
  const url = href.startsWith('http')
    ? new URL(href)
    : new URL(href, resolveApiOrigin());
  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== '') {
        url.searchParams.set(key, String(value));
      }
    }
  }
  return url.toString();
}

function buildAuthHeader(token: string, style: 'raw' | 'bearer'): string {
  const normalized = normalizeToken(token);
  return style === 'bearer' ? `Bearer ${normalized}` : normalized;
}

export async function api<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const { token, body, query, authStyle = 'raw', headers: extraHeaders, ...init } = options;

  const headers: Record<string, string> = {
    Accept: 'application/json',
    ...(extraHeaders as Record<string, string>),
  };

  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  if (token) {
    headers.Authorization = buildAuthHeader(token, authStyle);
  }

  const res = await fetch(buildUrl(path, query), {
    ...init,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (res.status === 204) {
    return undefined as T;
  }

  const text = await res.text();
  let parsed: unknown;
  try {
    parsed = text ? JSON.parse(text) : undefined;
  } catch {
    parsed = undefined;
  }

  if (!res.ok) {
    const err = parsed as ApiError | undefined;
    throw new SeasBrokerApiError(
      err?.message ?? `HTTP ${res.status}`,
      res.status,
      err?.data ?? {},
    );
  }

  return parsed as T;
}

/** Like `api()`, but posts a FormData body (multipart/form-data) - used for file uploads. */
export async function apiMultipart<T>(path: string, formData: FormData): Promise<T> {
  const res = await fetch(buildUrl(path), {
    method: 'POST',
    body: formData,
  });

  const text = await res.text();
  let parsed: unknown;
  try {
    parsed = text ? JSON.parse(text) : undefined;
  } catch {
    parsed = undefined;
  }

  if (!res.ok) {
    const err = parsed as ApiError | undefined;
    throw new SeasBrokerApiError(err?.message ?? `HTTP ${res.status}`, res.status, err?.data ?? {});
  }

  return parsed as T;
}

export interface ListQuery {
  page?: number;
  perPage?: number;
  filter?: string;
  sort?: string;
}

export function listCollection<T>(
  collection: string,
  query: ListQuery = {},
  token?: string,
  authStyle: 'raw' | 'bearer' = 'raw',
): Promise<PaginatedResponse<T>> {
  return api<PaginatedResponse<T>>(`/api/collections/${collection}/records`, {
    token,
    authStyle,
    query: {
      page: query.page ?? 1,
      perPage: query.perPage ?? 50,
      filter: query.filter,
      sort: query.sort,
    },
  });
}

export function getRecord<T>(
  collection: string,
  id: string,
  token?: string,
  authStyle: 'raw' | 'bearer' = 'raw',
): Promise<T> {
  return api<T>(`/api/collections/${collection}/records/${id}`, { token, authStyle });
}

export function createRecord<T>(
  collection: string,
  body: unknown,
  token?: string,
  authStyle: 'raw' | 'bearer' = 'raw',
): Promise<T> {
  return api<T>(`/api/collections/${collection}/records`, {
    method: 'POST',
    body,
    token,
    authStyle,
  });
}

export function updateRecord<T>(
  collection: string,
  id: string,
  body: unknown,
  token?: string,
  authStyle: 'raw' | 'bearer' = 'raw',
): Promise<T> {
  return api<T>(`/api/collections/${collection}/records/${id}`, {
    method: 'PATCH',
    body,
    token,
    authStyle,
  });
}

export function deleteRecord(
  collection: string,
  id: string,
  token?: string,
  authStyle: 'raw' | 'bearer' = 'raw',
): Promise<void> {
  return api<void>(`/api/collections/${collection}/records/${id}`, {
    method: 'DELETE',
    token,
    authStyle,
  });
}
