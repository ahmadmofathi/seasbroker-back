/**
 * CLI API test runner — run with: node scripts/test-api.mjs
 * Optional: pass superuser token as first arg for protected endpoint tests.
 */
const BASE = process.env.VITE_API_URL || 'http://seasbreaker.runasp.net';
const TOKEN = process.argv[2] || '';

const results = [];

async function test(name, method, path, { body, token, expectOk = true, expectStatus } = {}) {
  const headers = {};
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (token) headers['Authorization'] = `Bearer ${token}`;
  const start = Date.now();
  try {
    const res = await fetch(`${BASE}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
    const text = await res.text();
    let msg = text.slice(0, 150);
    try { const j = JSON.parse(text); if (j.message) msg = j.message; } catch {}
    const ok = expectStatus ? res.status === expectStatus : (expectOk ? res.ok : !res.ok || res.status === 401);
    const pass = expectStatus ? res.status === expectStatus : (name.includes('bad') ? res.status === 401 : res.ok);
    results.push({ name, status: res.status, pass, msg, ms: Date.now() - start });
    return { res, text };
  } catch (e) {
    results.push({ name, status: 0, pass: false, msg: e.message, ms: Date.now() - start });
    return null;
  }
}

console.log(`\nSeasBroker API Test — ${BASE}\n${'='.repeat(60)}`);
if (!TOKEN) console.log('No token provided — admin tests will expect 401\n');

// Public
await test('POST /api/get-chat-token', 'POST', '/api/get-chat-token');
const chatRes = await test('POST /api/get-chat-token (for message)', 'POST', '/api/get-chat-token');
let chatData = null;
try { chatData = JSON.parse(chatRes.text); } catch {}

await test('POST /api/quote', 'POST', '/api/quote', {
  body: {
    cargoType: 'Bulk', weight: 100, departurePort: 'Rotterdam',
    departureTime: '2026-07-01T00:00:00Z', arrivalPort: 'Singapore',
    arrivalTime: '2026-07-20T00:00:00Z', dimensions: '10x10x10',
    fname: 'CLI', lname: 'Test', email: 'cli-test@seasbroker.test', phoneNumber: '+1',
  },
});

if (chatData?.token) {
  await test('POST /api/collections/messages/records (anon)', 'POST', '/api/collections/messages/records', {
    body: { token: chatData.token, chatId: chatData.chatId, content: 'CLI test message' },
  });
}

await test('POST /api/auth/login (bad creds → 401)', 'POST', '/api/auth/login', {
  body: { email: 'bad@test.com', password: 'wrong' }, expectOk: false, expectStatus: 401,
});

await test('POST superuser auth (bad creds → 401)', 'POST', '/api/collections/_superusers/auth-with-password', {
  body: { identity: 'bad@test.com', password: 'wrong' }, expectStatus: 401,
});

// Protected
const adminTests = [
  ['GET /api/collections/chats/records', 'GET', '/api/collections/chats/records'],
  ['GET /api/collections/cargoListings/records', 'GET', '/api/collections/cargoListings/records'],
  ['GET /api/collections/vessels/records', 'GET', '/api/collections/vessels/records'],
  ['GET /api/collections/matches/records', 'GET', '/api/collections/matches/records'],
  ['GET /api/matches/pending-approval', 'GET', '/api/matches/pending-approval'],
  ['GET /api/matches/approved', 'GET', '/api/matches/approved'],
  ['GET /api/collections/matchingRules/records', 'GET', '/api/collections/matchingRules/records'],
  ['GET /api/notifications', 'GET', '/api/notifications'],
  ['GET /api/notifications/unread', 'GET', '/api/notifications/unread'],
  ['POST /api/matching/run', 'POST', '/api/matching/run', { body: {} }],
];

for (const [name, method, path, opts = {}] of adminTests) {
  await test(name, method, path, { ...opts, token: TOKEN, expectOk: !!TOKEN, expectStatus: TOKEN ? undefined : 401 });
}

if (TOKEN) {
  await test('POST /api/collections/_superusers/auth-refresh', 'POST', '/api/collections/_superusers/auth-refresh', { token: TOKEN });
}

console.log('\nResults:\n');
let pass = 0, fail = 0;
for (const r of results) {
  const icon = r.pass ? '✅' : '❌';
  if (r.pass) pass++; else fail++;
  console.log(`${icon} [${r.status}] ${r.name} (${r.ms}ms) — ${r.msg}`);
}
console.log(`\n${'='.repeat(60)}`);
console.log(`PASSED: ${pass}  FAILED: ${fail}  TOTAL: ${results.length}`);
if (!TOKEN) console.log('\nTip: node scripts/test-api.mjs <superuser-token> for full admin tests');
