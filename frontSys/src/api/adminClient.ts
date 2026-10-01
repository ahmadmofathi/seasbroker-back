import pb from '../utils/pocketbase';
import {
  API_BASE,
  api,
  createRecord,
  deleteRecord,
  getRecord,
  listCollection,
  SeasBrokerApiError,
  updateRecord,
} from './client';
import type { ListQuery } from './client';
import {
  clearSuperuserToken,
  getCollectionToken,
  getJwtToken,
  getRefreshToken,
  superuserRefresh,
  userRefresh,
} from './auth';
import { getAuthMode, normalizeToken, setAuthMode } from './authTokenStore';
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

/**
 * Retry once after a transient network failure (e.g. the tab was asleep/
 * backgrounded and the first request after waking hits a not-yet-reconnected
 * Wi-Fi/DNS). These surface as a raw fetch TypeError, not a SeasBrokerApiError
 * with an HTTP status — so they must not be confused with a real 401 and
 * must not clear the session, since the token itself is fine.
 */
async function withNetworkRetry<T>(fn: () => Promise<T>): Promise<T> {
  try {
    return await fn();
  } catch (error) {
    if (error instanceof SeasBrokerApiError) {
      throw error;
    }
    await new Promise((resolve) => setTimeout(resolve, 800));
    return await fn();
  }
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

  // Another request may have refreshed the session while this one was being
  // rejected - use its fresh token rather than spending the refresh token again.
  const current = getCollectionToken();
  if (current && !tokens.includes(current)) {
    for (const style of styles) {
      try {
        const result = await fn(current, style);
        setAuthMode(style);
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

  // Every known token was rejected — the access token has likely expired
  // (e.g. the tab was idle past its 60-minute lifetime). Try minting a new
  // one from the refresh token before giving up.
  if (getRefreshToken()) {
    try {
      const refreshed = await superuserRefresh();
      for (const style of styles) {
        try {
          const result = await fn(refreshed.token, style);
          setAuthMode(style);
          return result;
        } catch (error) {
          if (error instanceof SeasBrokerApiError && error.status === 401) {
            lastError = error;
            continue;
          }
          throw error;
        }
      }
    } catch {
      // Refresh token itself is invalid/expired — fall through below.
    }
  }

  // Refresh didn't help (or wasn't possible): the session is genuinely
  // over. Clear it so the reactive auth state redirects to /admin/login
  // instead of leaving every admin page silently broken.
  clearSuperuserToken();
  throw new SeasBrokerApiError(
    'Your session has expired. Please sign in again.',
    401,
    { cause: lastError?.message },
  );
}

export async function adminList<T>(collection: string, query: ListQuery = {}): Promise<T[]> {
  return withNetworkRetry(() =>
    withCollectionAuth(async (token, style) => {
      const result = await listCollection<T>(collection, query, token, style);
      return Array.isArray(result?.items) ? result.items : [];
    }),
  );
}

export async function adminGetOne<T>(collection: string, id: string): Promise<T> {
  return withNetworkRetry(() =>
    withCollectionAuth((token, style) => getRecord<T>(collection, id, token, style)),
  );
}

export async function adminCreate<T>(collection: string, body: unknown): Promise<T> {
  return withNetworkRetry(() =>
    withCollectionAuth((token, style) => createRecord<T>(collection, body, token, style)),
  );
}

export async function adminUpdate<T>(
  collection: string,
  id: string,
  body: unknown,
): Promise<T> {
  return withNetworkRetry(() =>
    withCollectionAuth((token, style) => updateRecord<T>(collection, id, body, token, style)),
  );
}

export async function adminDelete(collection: string, id: string): Promise<void> {
  await withNetworkRetry(() =>
    withCollectionAuth((token, style) => deleteRecord(collection, id, token, style)),
  );
}

export async function adminRequest<T>(
  path: string,
  options: {
    method?: string;
    body?: unknown;
    query?: Record<string, string | number | undefined>;
  } = {},
): Promise<T> {
  return withNetworkRetry(async () => {
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
      try {
        await userRefresh();
        token = getJwtToken() ?? getCollectionToken() ?? token;
        return await call(token, 'bearer');
      } catch {
        // Refresh token itself is invalid/expired — fall through below.
      }
    }

    clearSuperuserToken();
    throw new SeasBrokerApiError('Your session has expired. Please sign in again.', 401);
  });
}

export async function adminListPaginated<T>(
  path: string,
  query?: Record<string, string | number | undefined>,
): Promise<PaginatedResponse<T>> {
  return adminRequest<PaginatedResponse<T>>(path, { query });
}

/**
 * Downloads a file from an admin-only endpoint and hands it to the browser to save. A plain link
 * can't be used because the request needs the admin's token.
 */
export async function adminDownloadFile(path: string, fileName: string): Promise<void> {
  const request = (token: string) =>
    fetch(`${API_BASE}${path}`, { headers: { Authorization: `Bearer ${normalizeToken(token)}` } });

  let token = getJwtToken() ?? getCollectionToken();
  if (!token) {
    throw new SeasBrokerApiError('Please sign in to continue.', 401);
  }

  let res = await request(token);
  if (res.status === 401 && getRefreshToken()) {
    await userRefresh();
    token = getJwtToken() ?? getCollectionToken() ?? token;
    res = await request(token);
  }

  if (!res.ok) {
    let message = `Download failed (HTTP ${String(res.status)}).`;
    try {
      const body = (await res.json()) as { message?: string };
      if (body.message) message = body.message;
    } catch {
      // not a JSON error body - keep the generic message
    }
    throw new SeasBrokerApiError(message, res.status);
  }

  const url = URL.createObjectURL(await res.blob());
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

export { pb as adminPb };
