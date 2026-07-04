import { API_BASE, resolveApiOrigin } from './client';
import { getCollectionToken } from './auth';
import * as chatApi from './chat';
import * as cargoApi from './cargo';
import * as matchingApi from './matching';
import * as notificationsApi from './notifications';
import * as quoteApi from './quote';
import * as vesselsApi from './vessels';

export interface ApiTestResult {
  id: string;
  method: string;
  path: string;
  linkedPage: string;
  auth: 'none' | 'bearer' | 'superuser';
  status: number;
  ok: boolean;
  message: string;
  durationMs: number;
}

async function timed<T>(fn: () => Promise<T>): Promise<{ result: T; durationMs: number }> {
  const start = Date.now();
  const result = await fn();
  return { result, durationMs: Date.now() - start };
}

async function rawRequest(
  method: string,
  path: string,
  body?: unknown,
  token?: string,
): Promise<{ status: number; ok: boolean; message: string }> {
  const headers: Record<string, string> = {};
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(
    path.startsWith('http') ? path : `${API_BASE || resolveApiOrigin()}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  const text = await res.text();
  let message = text.slice(0, 200);
  try {
    const parsed = JSON.parse(text) as { message?: string };
    if (parsed.message) message = parsed.message;
  } catch {
    // keep raw text
  }

  return { status: res.status, ok: res.ok, message };
}

function result(
  id: string,
  method: string,
  path: string,
  linkedPage: string,
  auth: ApiTestResult['auth'],
  status: number,
  ok: boolean,
  message: string,
  durationMs: number,
): ApiTestResult {
  return { id, method, path, linkedPage, auth, status, ok, message, durationMs };
}

export async function runAllApiTests(): Promise<ApiTestResult[]> {
  const results: ApiTestResult[] = [];
  const token = getCollectionToken() ?? undefined;

  const run = async (
    id: string,
    method: string,
    path: string,
    linkedPage: string,
    auth: ApiTestResult['auth'],
    fn: () => Promise<{ status: number; ok: boolean; message: string }>,
  ) => {
    try {
      const { result: r, durationMs } = await timed(fn);
      results.push(result(id, method, path, linkedPage, auth, r.status, r.ok, r.message, durationMs));
    } catch (e) {
      results.push(
        result(id, method, path, linkedPage, auth, 0, false, e instanceof Error ? e.message : String(e), 0),
      );
    }
  };

  // ── Public endpoints ──
  await run('get-chat-token', 'POST', '/api/get-chat-token', 'Chat widget (all pages)', 'none', async () => {
    const { result: r, durationMs } = await timed(() => chatApi.getChatToken());
    return { status: 200, ok: true, message: `chatId: ${r.chatId}`, durationMs };
  });

  let chatTokenData: { token: string; chatId: string } | null = null;
  try {
    chatTokenData = await chatApi.getChatToken();
  } catch { /* handled above */ }

  await run('quote', 'POST', '/api/quote', '/request_quote', 'none', async () => {
    const { result: r, durationMs } = await timed(() =>
      quoteApi.submitQuote({
        cargoType: 'Bulk',
        weight: 100,
        departurePort: 'Rotterdam',
        departureTime: '2026-07-01T00:00:00Z',
        arrivalPort: 'Singapore',
        arrivalTime: '2026-07-20T00:00:00Z',
        dimensions: '10x10x10',
        fname: 'API',
        lname: 'Test',
        email: 'apitest@seasbroker.test',
        phoneNumber: '+1000000000',
        additionalInfo: 'Automated API test',
      }),
    );
    return { status: 200, ok: true, message: r.message, durationMs };
  });

  if (chatTokenData) {
    await run('anonymous-message', 'POST', '/api/collections/messages/records', 'Chat widget', 'none', async () => {
      const { result: r, durationMs } = await timed(() =>
        chatApi.sendAnonymousMessage({
          token: chatTokenData!.token,
          chatId: chatTokenData!.chatId,
          content: `[API test] ${new Date().toISOString()}`,
        }),
      );
      return { status: 200, ok: true, message: `message id: ${r.id}`, durationMs };
    });
  }

  await run('auth-login-bad', 'POST', '/api/auth/login', '/signIn (disabled)', 'none', () =>
    rawRequest('POST', '/api/auth/login', { email: 'invalid@test.com', password: 'wrong' }),
  );

  await run('superuser-login-bad', 'POST', '/api/collections/_superusers/auth-with-password', '/admin/login', 'none', () =>
    rawRequest('POST', '/api/collections/_superusers/auth-with-password', {
      identity: 'invalid@test.com',
      password: 'wrong',
    }),
  );

  if (!token) {
    const protectedEndpoints: Array<[string, string, string, string]> = [
      ['chats-list', 'GET', '/api/collections/chats/records', '/admin/chats'],
      ['quotes-list', 'GET', '/api/collections/requestedQuotes/records', '/admin/quotes'],
      ['cargo-list', 'GET', '/api/collections/cargoListings/records', '/admin/cargo'],
      ['vessels-list', 'GET', '/api/collections/vessels/records', '/admin/vessels'],
      ['matches-list', 'GET', '/api/collections/matches/records', '/admin/matching'],
      ['pending-approval', 'GET', '/api/matches/pending-approval', '/admin/matching'],
      ['approved-matches', 'GET', '/api/matches/approved', '/admin/matching'],
      ['matching-rules', 'GET', '/api/collections/matchingRules/records', '/admin/matching'],
      ['notifications', 'GET', '/api/notifications', '/admin/notifications'],
      ['notifications-unread', 'GET', '/api/notifications/unread', '/admin/notifications'],
      ['auth-refresh', 'POST', '/api/collections/_superusers/auth-refresh', '/admin (session)'],
    ];
    for (const [id, method, path, page] of protectedEndpoints) {
      await run(id, method, path, page, 'superuser', () =>
        rawRequest(method, path, method === 'POST' ? {} : undefined, token),
      );
    }
    return results;
  }

  // ── Authenticated endpoints ──
  await run('auth-refresh', 'POST', '/api/collections/_superusers/auth-refresh', '/admin (session)', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => import('./auth').then((m) => m.superuserRefresh()));
    return { status: 200, ok: true, message: `token refreshed for ${r.record.email}`, durationMs };
  });

  await run('chats-list', 'GET', '/api/collections/chats/records', '/admin/chats', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => chatApi.listChats());
    return { status: 200, ok: true, message: `${r.length} chats`, durationMs };
  });

  await run('quotes-list', 'GET', '/api/collections/requestedQuotes/records', '/admin/quotes', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => quoteApi.listRequestedQuotes());
    return { status: 200, ok: true, message: `${r.length} requests`, durationMs };
  });

  await run('cargo-list', 'GET', '/api/collections/cargoListings/records', '/admin/cargo', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => cargoApi.listCargoListings());
    return { status: 200, ok: true, message: `${r.length} listings`, durationMs };
  });

  await run('vessels-list', 'GET', '/api/collections/vessels/records', '/admin/vessels', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => vesselsApi.listVessels());
    return { status: 200, ok: true, message: `${r.length} vessels`, durationMs };
  });

  await run('matches-list', 'GET', '/api/collections/matches/records', '/admin/matching', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => matchingApi.listMatches());
    return { status: 200, ok: true, message: `${r.length} matches`, durationMs };
  });

  await run('pending-approval', 'GET', '/api/matches/pending-approval', '/admin/matching', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => matchingApi.listPendingApproval());
    return { status: 200, ok: true, message: `${r.length} pending`, durationMs };
  });

  await run('approved-matches', 'GET', '/api/matches/approved', '/admin/matching', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => matchingApi.listApprovedMatches());
    return { status: 200, ok: true, message: `${r.length} approved`, durationMs };
  });

  await run('matching-rules', 'GET', '/api/collections/matchingRules/records', '/admin/matching', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => matchingApi.listMatchingRules());
    return { status: 200, ok: true, message: `${r.length} rules`, durationMs };
  });

  await run('matching-run', 'POST', '/api/matching/run', '/admin/matching', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => matchingApi.runMatching({}));
    return {
      status: 200,
      ok: true,
      message: `created: ${r.matchesCreated}, skipped: ${r.matchesSkipped}`,
      durationMs,
    };
  });

  await run('notifications', 'GET', '/api/notifications', '/admin/notifications', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => notificationsApi.listNotifications());
    return { status: 200, ok: true, message: `${r.totalItems} total`, durationMs };
  });

  await run('notifications-unread', 'GET', '/api/notifications/unread', '/admin/notifications', 'superuser', async () => {
    const { result: r, durationMs } = await timed(() => notificationsApi.listUnreadNotifications());
    return { status: 200, ok: true, message: `${r.length} unread`, durationMs };
  });

  const chats = await chatApi.listChats().catch(() => []);
  if (chats.length > 0) {
    await run('messages-list', 'GET', '/api/collections/messages/records', '/admin/chats', 'superuser', async () => {
      const { result: r, durationMs } = await timed(() => chatApi.listMessages(chats[0].id));
      return { status: 200, ok: true, message: `${r.length} messages`, durationMs };
    });
  }

  return results;
}

/** Friendly labels for health-check UI — no endpoint paths exposed */
export const HEALTH_CHECK_LABELS: Record<string, string> = {
  'get-chat-token': 'Live Chat Service',
  quote: 'Quote Requests',
  'anonymous-message': 'Visitor Messaging',
  'auth-login-bad': 'User Authentication',
  'superuser-login-bad': 'Admin Authentication',
  'auth-refresh': 'Session Refresh',
  'chats-list': 'Chat Management',
  'quotes-list': 'Public Requests',
  'messages-list': 'Message History',
  'cargo-list': 'Cargo Listings',
  'vessels-list': 'Vessel Fleet',
  'matches-list': 'Match Records',
  'pending-approval': 'Approval Queue',
  'approved-matches': 'Approved Matches',
  'matching-rules': 'Matching Rules',
  'matching-run': 'Matching Engine',
  notifications: 'Notifications',
  'notifications-unread': 'Unread Alerts',
};
