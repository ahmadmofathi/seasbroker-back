/**
 * Probe superuser auth — usage:
 *   node scripts/probe-auth.mjs <email> <password>
 */
const BASE = process.env.VITE_API_URL || 'https://appapi.seasbroker.com';
const [email, password] = process.argv.slice(2);

if (!email || !password) {
  console.error('Usage: node scripts/probe-auth.mjs <email> <password>');
  process.exit(1);
}

async function req(method, path, { body, auth } = {}) {
  const headers = { Accept: 'application/json' };
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (auth) headers.Authorization = auth;
  const res = await fetch(`${BASE}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    redirect: 'manual',
  });
  const text = await res.text();
  return { status: res.status, location: res.headers.get('location'), text: text.slice(0, 300) };
}

const login = await req('POST', '/api/collections/_superusers/auth-with-password', {
  body: { identity: email, password },
});
console.log('\n=== LOGIN ===');
console.log(login);

let token;
try {
  const j = JSON.parse(login.text.startsWith('{') ? login.text : '{}');
  token = j.token || j.Token;
} catch {
  token = null;
}

if (!token) {
  console.log('\nNo token — cannot continue.');
  process.exit(1);
}

console.log('\nToken prefix:', token.slice(0, 40) + '...');

const tests = [
  ['records raw', 'GET', `/api/collections/chats/records?page=1&perPage=1`, token],
  ['records bearer', 'GET', `/api/collections/chats/records?page=1&perPage=1`, `Bearer ${token}`],
  ['refresh raw', 'POST', '/api/collections/_superusers/auth-refresh', token],
  ['refresh bearer', 'POST', '/api/collections/_superusers/auth-refresh', `Bearer ${token}`],
];

for (const [name, method, path, auth] of tests) {
  const r = await req(method, path, { auth });
  console.log(`\n=== ${name} ===`);
  console.log(r);
}
