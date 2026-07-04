import { adminGetOne, adminList, adminRequest, adminUpdate } from './adminClient';
import type {
  ManualMatchBody,
  MatchActionBody,
  MatchRecord,
  MatchingRuleRecord,
  RunMatchingBody,
  RunMatchingResponse,
} from './types';

/** API may return a bare array or a PocketBase-style { items: [] } payload. */
function asList<T>(data: unknown): T[] {
  if (Array.isArray(data)) return data as T[];
  if (data && typeof data === 'object' && Array.isArray((data as { items?: unknown }).items)) {
    return (data as { items: T[] }).items;
  }
  return [];
}

export async function runMatching(body: RunMatchingBody): Promise<RunMatchingResponse> {
  return adminRequest<RunMatchingResponse>('/api/matching/run', { method: 'POST', body });
}

export async function createManualMatch(body: ManualMatchBody): Promise<MatchRecord> {
  return adminRequest<MatchRecord>('/api/matching/manual', { method: 'POST', body });
}

export async function listMatches(
  filter?: string,
  page = 1,
  perPage = 50,
): Promise<MatchRecord[]> {
  return asList<MatchRecord>(await adminList<MatchRecord>('matches', { page, perPage, filter }));
}

export async function getMatch(id: string): Promise<MatchRecord> {
  return adminGetOne<MatchRecord>('matches', id);
}

export async function expireMatch(id: string): Promise<unknown> {
  return adminRequest(`/api/matches/${id}/expire`, { method: 'POST' });
}

export async function listMatchingRules(): Promise<MatchingRuleRecord[]> {
  return asList<MatchingRuleRecord>(
    await adminList<MatchingRuleRecord>('matchingRules', { page: 1, perPage: 50 }),
  );
}

export async function updateMatchingRule(
  id: string,
  body: Partial<MatchingRuleRecord>,
): Promise<MatchingRuleRecord> {
  return adminUpdate<MatchingRuleRecord>('matchingRules', id, body);
}

export async function listPendingApproval(): Promise<MatchRecord[]> {
  return asList<MatchRecord>(await adminRequest<unknown>('/api/matches/pending-approval'));
}

export async function listApprovedMatches(): Promise<MatchRecord[]> {
  return asList<MatchRecord>(await adminRequest<unknown>('/api/matches/approved'));
}

export async function approveMatch(id: string, body: MatchActionBody): Promise<MatchRecord> {
  return adminRequest<MatchRecord>(`/api/matches/${id}/approve`, { method: 'POST', body });
}

export async function rejectMatch(id: string, body: MatchActionBody): Promise<MatchRecord> {
  return adminRequest<MatchRecord>(`/api/matches/${id}/reject`, { method: 'POST', body });
}

export async function cancelMatch(id: string, body: MatchActionBody): Promise<MatchRecord> {
  return adminRequest<MatchRecord>(`/api/matches/${id}/cancel`, { method: 'POST', body });
}

export async function completeMatch(id: string, body: MatchActionBody): Promise<MatchRecord> {
  return adminRequest<MatchRecord>(`/api/matches/${id}/complete`, { method: 'POST', body });
}
