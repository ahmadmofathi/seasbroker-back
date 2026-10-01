import { api } from './client';
import type { RequestTracking } from './types';

/** Public lookup of a request by its tracking number (or cargo listing reference) and the customer's email. */
export function trackRequest(number: string, email: string): Promise<RequestTracking> {
  // POST: keeps the email out of the URL (and so out of server logs and browser history)
  return api<RequestTracking>('/api/track', { method: 'POST', body: { number: number.trim(), email: email.trim() } });
}
