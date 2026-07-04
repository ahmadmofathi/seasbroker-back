import pb from '../utils/pocketbase';
import {
  api,
  createRecord,
  deleteRecord,
  getRecord,
  listCollection,
  SeasBrokerApiError,
  updateRecord,
} from './client';
import type { ListQuery } from './client';
import { getCollectionToken, getJwtToken, getRefreshToken, userRefresh } from './auth';
import { getAuthMode, setAuthMode } from './authTokenStore';
import type { PaginatedResponse } from './types';

export function resetLoginRedirectFlag(): void {
  // no-op
}

export function isAdminAuthenticated(): boolean {
  return !!getCollectionToken();
}

function requireCollectionToken(): string {
  const token = getCollectionToken();
  if (!token) {
    throw new SeasBrokerApiError('Please sign in to continue.', 401);
  }
  return token;
}

async function withCollectionAuth<T>(
  fn: (token: string, style: 'raw' | 'bearer') => Promise<T>,
): Promise<T> {
  const pbToken = requireCollectionToken();
  const preferred = getAuthMode();
  const styles: Array<'raw' | 'bearer'> =
    preferred === 'bearer' ? ['bearer', 'raw'] : ['raw', 'bearer'];

  const tokens = [pbToken];
  const jwt = getJwtToken();
  if (jwt && jwt !== pbToken) {
    tokens.push(jwt);
  }

  let lastError: SeasBrokerApiError | undefined;

  for (const token of tokens) {
    for (const style of styles) {
      try {
        const result = await fn(token, style);
        if (token === pbToken) {
          setAuthMode(style);
        }
        return result;
      } catch (error) {
        if (error instanceof SeasBrokerApiError && error.status === 401) {
          lastError = error;
          continue;
        }
        throw error;
      }
    }
  }

  throw lastError ?? new SeasBrokerApiError('Could not access this resource.', 401);
}

export async function adminList<T>(collection: string, query: ListQuery = {}): Promise<T[]> {
  return withCollectionAuth(async (token, style) => {
    const result = await listCollection<T>(collection, query, token, style);
    return Array.isArray(result?.items) ? result.items : [];
  });
}

export async function adminGetOne<T>(collection: string, id: string): Promise<T> {
  return withCollectionAuth((token, style) => getRecord<T>(collection, id, token, style));
}

export async function adminCreate<T>(collection: string, body: unknown): Promise<T> {
  return withCollectionAuth((token, style) =>
    createRecord<T>(collection, body, token, style),
  );
}

export async function adminUpdate<T>(
  collection: string,
  id: string,
  body: unknown,
): Promise<T> {
  return withCollectionAuth((token, style) =>
    updateRecord<T>(collection, id, body, token, style),
  );
}

export async function adminDelete(collection: string, id: string): Promise<void> {
  await withCollectionAuth((token, style) => deleteRecord(collection, id, token, style));
}

export async function adminRequest<T>(
  path: string,
  options: {
    method?: string;
    body?: unknown;
    query?: Record<string, string | number | undefined>;
  } = {},
): Promise<T> {
  let token = getJwtToken() ?? getCollectionToken();
  if (!token) {
    throw new SeasBrokerApiError('Please sign in to continue.', 401);
  }

  const call = (authToken: string, authStyle: 'bearer' | 'raw') =>
    api<T>(path, {
      method: options.method ?? 'GET',
      body: options.body,
      query: options.query,
      token: authToken,
      authStyle,
    });

  for (const authStyle of ['bearer', 'raw'] as const) {
    try {
      return await call(token, authStyle);
    } catch (error) {
      if (!(error instanceof SeasBrokerApiError && error.status === 401)) {
        throw error;
      }
    }
  }

  if (getRefreshToken()) {
    await userRefresh();
    token = getJwtToken() ?? getCollectionToken() ?? token;
    return call(token, 'bearer');
  }

  throw new SeasBrokerApiError('Could not access this resource.', 401);
}

export async function adminListPaginated<T>(
  path: string,
  query?: Record<string, string | number | undefined>,
): Promise<PaginatedResponse<T>> {
  return adminRequest<PaginatedResponse<T>>(path, { query });
}

export { pb as adminPb };
