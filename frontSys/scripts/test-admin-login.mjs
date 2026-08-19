/**
 * Test admin login flow — set env vars first:
 *   set ADMIN_EMAIL=your@email.com
 *   set ADMIN_PASSWORD=yourpassword
 *   node scripts/test-admin-login.mjs
 */
const BASE = process.env.VITE_API_URL || 'https://api.seasbroker.com';
const email = process.env.ADMIN_EMAIL;
const password = process.env.ADMIN_PASSWORD;

if (!email || !password) {
  console.error('Set ADMIN_EMAIL and ADMIN_PASSWORD environment variables.');
  process.exit(1);
}

async function probe(path, token, style) {
  const auth = style === 'bearer' ? `Bearer ${token}` : token;
  const r = await fetch(`${BASE}${path}`, {
    headers: { Accept: 'application/json', Authorization: auth },
  });
  const text = await r.text();
  return { status: r.status, body: text.slice(0, 120) };
}

async function login(path, body) {
  const r = await fetch(`${BASE}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify(body),
  });
  const data = await r.json();
  return { ok: r.ok, status: r.status, data };
}

console.log(`Testing admin login against ${BASE}\n`);

const jwt = await login('/api/auth/login', { email, password });
console.log('JWT login:', jwt.status, jwt.ok ? 'OK' : jwt.data?.message);
if (jwt.ok && jwt.data.accessToken) {
  for (const style of ['bearer', 'raw']) {
    const p = await probe('/api/collections/chats/records?page=1&perPage=1', jwt.data.accessToken, style);
    console.log(`  chats (${style}):`, p.status, p.body);
  }
}

const pb = await login('/api/collections/_superusers/auth-with-password', { identity: email, password });
console.log('\nSuperuser login:', pb.status, pb.ok ? 'OK' : pb.data?.message);
if (pb.ok && pb.data.token) {
  for (const style of ['bearer', 'raw']) {
    const p = await probe('/api/collections/chats/records?page=1&perPage=1', pb.data.token, style);
    console.log(`  chats (${style}):`, p.status, p.body);
  }
}
