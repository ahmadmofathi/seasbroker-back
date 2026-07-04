import pb from '../utils/pocketbase';
import { ClientResponseError } from 'pocketbase';
import type { SuperuserAuthResponse, UserAuthResponse } from './types';
import {
  api,
  listCollection,
  SeasBrokerApiError,
} from './client';
import {
  clearAuthMode,
  clearPbSdkStorage,
  clearPbToken,
  clearStoredToken,
  getAuthMode,
  getPbToken,
  setAuthMode,
  setPbToken,
  setStoredToken,
} from './authTokenStore';

const ACCESS_TOKEN_KEY = 'seasbroker_access_token';
const REFRESH_TOKEN_KEY = 'seasbroker_refresh_token';

function readJwtField(response: Record<string, unknown>, key: string): string | undefined {
  const pascal = key.charAt(0).toUpperCase() + key.slice(1);
  const value = response[key] ?? response[pascal];
  return typeof value === 'string' ? value : undefined;
}

function mapPbError(error: unknown): never {
  if (error instanceof ClientResponseError) {
    throw new SeasBrokerApiError(
      error.message || 'Request failed',
      error.status,
      (error.response as Record<string, unknown>) ?? {},
    );
  }
  throw error;
}

/** PocketBase token for /api/collections/* — JWT alone is NOT valid here. */
export function getSuperuserToken(): string | null {
  return pb.authStore.token || getPbToken();
}

export function getCollectionToken(): string | null {
  return getSuperuserToken();
}

export function getJwtToken(): string | null {
  return getAccessToken();
}

export function setSuperuserToken(token: string): void {
  setPbToken(token);
  setStoredToken(token);
  pb.authStore.save(token, pb.authStore.record);
}

export function clearSuperuserToken(): void {
  pb.authStore.clear();
  clearStoredToken();
  clearPbToken();
  clearAuthMode();
  clearPbSdkStorage();
  clearUserTokens();
}

/** Restore PocketBase auth store from localStorage on page load */
export function restoreSuperuserSession(): void {
  const pbToken = getPbToken();
  if (pbToken && pb.authStore.token !== pbToken) {
    pb.authStore.save(pbToken);
  }
}

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function setUserTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

export function clearUserTokens(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

async function detectWorkingAuthStyle(pbToken: string): Promise<'raw' | 'bearer'> {
  const preferred = getAuthMode();
  const order: Array<'raw' | 'bearer'> =
    preferred === 'bearer' ? ['bearer', 'raw'] : ['raw', 'bearer'];

  for (const style of order) {
    try {
      await listCollection('chats', { page: 1, perPage: 1 }, pbToken, style);
      return style;
    } catch (error) {
      if (!(error instanceof SeasBrokerApiError && error.status === 401)) {
        throw error;
      }
    }
  }

  throw new SeasBrokerApiError(
    'Server rejected the auth token on all admin APIs (HTTP 401 → /Account/Login). ' +
      'This is a backend configuration issue: ASP.NET must accept Bearer/JWT on /api/* routes. ' +
      'Contact the backend team — the frontend cannot fix this.',
    401,
    { code: 'BACKEND_AUTH_MISCONFIGURED' },
  );
}

/**
 * Admin login:
 * 1) PocketBase superuser auth (required for /api/collections/*)
 * 2) JWT login (optional, for /api/notifications, /api/matching, etc.)
 */
export async function superuserLogin(
  identity: string,
  password: string,
): Promise<SuperuserAuthResponse> {
  clearSuperuserToken();

  let auth;
  try {
    auth = await pb.collection('_superusers').authWithPassword(identity, password);
  } catch (error) {
    mapPbError(error);
  }

  const pbToken = pb.authStore.token;
  if (!pbToken) {
    throw new SeasBrokerApiError('Authentication failed. No token received.', 401);
  }

  const authStyle = await detectWorkingAuthStyle(pbToken);

  setPbToken(pbToken);
  setStoredToken(pbToken);
  setAuthMode(authStyle);

  try {
    const userResponse = await api<UserAuthResponse & Record<string, unknown>>(
      '/api/auth/login',
      { method: 'POST', body: { email: identity, password } },
    );
    const accessToken = readJwtField(userResponse, 'accessToken');
    const refreshToken = readJwtField(userResponse, 'refreshToken');
    if (accessToken && refreshToken) {
      setUserTokens(accessToken, refreshToken);
    }
  } catch {
    // JWT optional — PocketBase token is enough for collections
  }

  return {
    token: pbToken,
    record: auth!.record as SuperuserAuthResponse['record'],
  };
}

export async function superuserRefresh(): Promise<SuperuserAuthResponse> {
  const token = pb.authStore.token || getPbToken();
  if (!token) {
    throw new SeasBrokerApiError('Not authenticated', 401);
  }

  if (pb.authStore.token !== token) {
    pb.authStore.save(token);
  }

  return { token, record: pb.authStore.record as SuperuserAuthResponse['record'] };
}

export async function userLogin(email: string, password: string): Promise<UserAuthResponse> {
  const response = await api<UserAuthResponse>('/api/auth/login', {
    method: 'POST',
    body: { email, password },
    authStyle: 'bearer',
  });
  setUserTokens(response.accessToken, response.refreshToken);
  return response;
}

export async function userRefresh(refreshToken?: string): Promise<UserAuthResponse> {
  const token = refreshToken ?? getRefreshToken();
  if (!token) {
    throw new Error('No refresh token available');
  }
  const response = await api<UserAuthResponse & Record<string, unknown>>('/api/auth/refresh', {
    method: 'POST',
    body: { refreshToken: token },
  });

  const accessToken = readJwtField(response, 'accessToken') ?? response.accessToken;
  const newRefresh = readJwtField(response, 'refreshToken') ?? response.refreshToken;
  setUserTokens(accessToken, newRefresh);

  return { ...response, accessToken, refreshToken: newRefresh };
}

export async function userLogout(): Promise<void> {
  const accessToken = getAccessToken();
  const refreshToken = getRefreshToken();
  if (accessToken && refreshToken) {
    await api<void>('/api/auth/logout', {
      method: 'POST',
      token: accessToken,
      authStyle: 'bearer',
      body: { refreshToken },
    });
  }
  clearUserTokens();
}
