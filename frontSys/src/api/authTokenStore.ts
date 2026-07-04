const SUPERUSER_TOKEN_KEY = 'seasbroker_superuser_token';
const POCKETBASE_TOKEN_KEY = 'seasbroker_pocketbase_token';
const AUTH_MODE_KEY = 'seasbroker_auth_mode';

export type AdminAuthMode = 'raw' | 'bearer';

let memoryToken: string | null = null;
let memoryAuthMode: AdminAuthMode = 'raw';

export function normalizeToken(token: string): string {
  return token.replace(/^Bearer\s+/i, '').trim();
}

export function isValidToken(token: string | null | undefined): token is string {
  if (!token) return false;
  const normalized = normalizeToken(token);
  return normalized.length > 0 && normalized !== 'undefined' && normalized !== 'null';
}

export function setStoredToken(token: string): void {
  const normalized = normalizeToken(token);
  if (!isValidToken(normalized)) return;
  memoryToken = normalized;
  localStorage.setItem(SUPERUSER_TOKEN_KEY, normalized);
}

export function getStoredToken(): string | null {
  if (isValidToken(memoryToken)) return memoryToken;
  const fromStorage = localStorage.getItem(SUPERUSER_TOKEN_KEY);
  if (isValidToken(fromStorage)) {
    memoryToken = normalizeToken(fromStorage);
    return memoryToken;
  }
  memoryToken = null;
  return null;
}

export function clearStoredToken(): void {
  memoryToken = null;
  localStorage.removeItem(SUPERUSER_TOKEN_KEY);
}

export function setPbToken(token: string): void {
  const normalized = normalizeToken(token);
  if (!isValidToken(normalized)) return;
  localStorage.setItem(POCKETBASE_TOKEN_KEY, normalized);
}

export function getPbToken(): string | null {
  const fromStorage = localStorage.getItem(POCKETBASE_TOKEN_KEY);
  if (isValidToken(fromStorage)) return normalizeToken(fromStorage);
  return null;
}

export function clearPbToken(): void {
  localStorage.removeItem(POCKETBASE_TOKEN_KEY);
}

export function setAuthMode(mode: AdminAuthMode): void {
  memoryAuthMode = mode;
  localStorage.setItem(AUTH_MODE_KEY, mode);
}

export function getAuthMode(): AdminAuthMode {
  const stored = localStorage.getItem(AUTH_MODE_KEY);
  if (stored === 'raw' || stored === 'bearer') {
    memoryAuthMode = stored;
    return stored;
  }
  return memoryAuthMode;
}

export function clearAuthMode(): void {
  memoryAuthMode = 'raw';
  localStorage.removeItem(AUTH_MODE_KEY);
}

/** Remove PocketBase SDK persisted auth keys (can hold stale/wrong tokens). */
export function clearPbSdkStorage(): void {
  if (typeof localStorage === 'undefined') return;
  const keys: string[] = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (key && (/^pb_/i.test(key) || key.includes('pocketbase'))) {
      keys.push(key);
    }
  }
  keys.forEach((key) => localStorage.removeItem(key));
}
