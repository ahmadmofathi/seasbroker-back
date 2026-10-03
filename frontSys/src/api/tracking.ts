import { api } from './client';
import type { RequestEditForm, RequestTracking } from './types';
import type { SubmitFormValues } from './forms';

/** Public lookup of a request by its tracking number (or cargo listing reference) and the customer's email. */
export function trackRequest(number: string, email: string): Promise<RequestTracking> {
  // POST: keeps the email out of the URL (and so out of server logs and browser history)
  return api<RequestTracking>('/api/track', { method: 'POST', body: { number: number.trim(), email: email.trim() } });
}

/** Loads the customer's own request in the form it was registered with. 409 once the team has accepted it. */
export function loadRequestForEdit(number: string, email: string): Promise<RequestEditForm> {
  return api<RequestEditForm>('/api/track/edit-form', {
    method: 'POST',
    body: { number: number.trim(), email: email.trim() },
  });
}

/** Saves the customer's changes to their own request. */
export async function saveRequestEdit(number: string, email: string, values: SubmitFormValues): Promise<void> {
  await api<undefined>('/api/track/edit', {
    method: 'PUT',
    body: { number: number.trim(), email: email.trim(), values },
  });
}
