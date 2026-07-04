import type { SuperuserAuthResponse } from './types';
import { isValidToken, normalizeToken } from './authTokenStore';

/** Handles camelCase, PascalCase, or nested API responses */
export function extractSuperuserToken(response: SuperuserAuthResponse & Record<string, unknown>): string {
  const data = response.data as Record<string, unknown> | undefined;

  const candidates = [
    response.token,
    response['Token'],
    response['accessToken'],
    response['AccessToken'],
    data?.token,
    data?.Token,
    data?.accessToken,
    data?.AccessToken,
  ];

  for (const candidate of candidates) {
    if (typeof candidate === 'string' && isValidToken(candidate)) {
      return normalizeToken(candidate);
    }
  }

  throw new Error('Authentication failed. No valid token received.');
}
