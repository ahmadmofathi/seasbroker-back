/**
 * Probe which endpoints accept PB token vs JWT.
 * Usage: ADMIN_EMAIL=... ADMIN_PASSWORD=... node scripts/probe-all-auth.mjs
 */
const BASE = process.env.VITE_API_URL || 'https://api.seasbroker.com';
const email = process.env.ADMIN_EMAIL;
const password = process.env.ADMIN_PASSWORD;

if (!email || !password) {
  console.error('Set ADMIN_EMAIL and ADMIN_PASSWORD.');
  process.exit(1);
}

async function loginPb() {
  const r = await fetch(`${BASE}/api/collections/_superusers/auth-with-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify({ identity: email, password }),
  });
  const data = await r.json();
  return { ok: r.ok, token: data.token };
}

async function loginJwt() {
  const r = await fetch(`${BASE}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  const data = await r.json();
  return { ok: r.ok, token: data.accessToken || data.AccessToken };
}

async function probe(label, method, path, token, style, body) {
  const headers = { Accept: 'application/json' };
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (token) {
    headers.Authorization = style === 'bearer' ? `Bearer ${token}` : token;
  }
  const r = await fetch(`${BASE}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    redirect: 'manual',
  });
  const loc = r.headers.get('location');
  const text = await r.text();
  let msg = text.slice(0, 80).replace(/\s+/g, ' ');
  if (loc) msg = `→ ${loc.split('?')[0]}`;
  const ok = r.status >= 200 && r.status < 300;
  console.log(`${ok ? '✓' : '✗'} ${label.padEnd(42)} ${r.status} ${msg}`);
  return { ok, status: r.status };
}

const endpoints = [
  ['GET', '/api/collections/chats/records?page=1&perPage=1'],
  ['GET', '/api/collections/cargoListings/records?page=1&perPage=1'],
  ['GET', '/api/collections/vessels/records?page=1&perPage=1'],
  ['GET', '/api/collections/matches/records?page=1&perPage=1'],
  ['GET', '/api/matches/pending-approval'],
  ['GET', '/api/matches/approved'],
  ['GET', '/api/notifications'],
  ['GET', '/api/notifications/unread'],
  ['POST', '/api/matching/run', {}],
];

console.log(`Target: ${BASE}\n`);

const pb = await loginPb();
const jwt = await loginJwt();
console.log('PB login:', pb.ok ? 'OK' : 'FAIL');
console.log('JWT login:', jwt.ok ? 'OK' : 'FAIL');
console.log('');

for (const [method, path, body] of endpoints) {
  const name = `${method} ${path.split('?')[0]}`;
  console.log(`--- ${name} ---`);
  if (pb.token) {
    await probe('PB raw', method, path, pb.token, 'raw', body);
    await probe('PB bearer', method, path, pb.token, 'bearer', body);
  }
  if (jwt.token) {
    await probe('JWT bearer', method, path, jwt.token, 'bearer', body);
    await probe('JWT raw', method, path, jwt.token, 'raw', body);
  }
  console.log('');
}
