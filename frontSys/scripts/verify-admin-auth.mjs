/**
 * Verify admin auth end-to-end.
 *
 * Usage (PowerShell):
 *   $env:ADMIN_EMAIL="your@email.com"
 *   $env:ADMIN_PASSWORD="yourpassword"
 *   npm run verify:admin
 */
import PocketBase from 'pocketbase';

const BASE = process.env.VITE_API_URL || 'http://seasbreaker.runasp.net';
const email = process.env.ADMIN_EMAIL;
const password = process.env.ADMIN_PASSWORD;

if (!email || !password) {
  console.error('Set ADMIN_EMAIL and ADMIN_PASSWORD environment variables.');
  process.exit(1);
}

async function fetchRecords(token, style) {
  const auth = style === 'bearer' ? `Bearer ${token}` : token;
  const res = await fetch(`${BASE}/api/collections/chats/records?page=1&perPage=1`, {
    headers: { Accept: 'application/json', Authorization: auth },
    redirect: 'manual',
  });
  const text = await res.text();
  return {
    style,
    status: res.status,
    location: res.headers.get('location'),
    body: text.slice(0, 120),
  };
}

function printBackendDiagnosis() {
  console.error(`
╔══════════════════════════════════════════════════════════════════╗
║  BACKEND ISSUE — NOT A FRONTEND BUG                              ║
╠══════════════════════════════════════════════════════════════════╣
║  Login works, but ALL protected /api/* routes return 401 and     ║
║  redirect to /Account/Login (ASP.NET cookie auth challenge).     ║
║                                                                  ║
║  The server issues JWT tokens but does NOT validate them on:     ║
║    • /api/collections/*/records                                  ║
║    • /api/notifications                                          ║
║    • /api/matches/*                                                ║
║                                                                  ║
║  Backend team must configure JWT Bearer auth for /api/* OR       ║
║  exempt PocketBase routes from ASP.NET cookie middleware.        ║
║                                                                  ║
║  Run: npm run probe:all   (full endpoint matrix)                 ║
╚══════════════════════════════════════════════════════════════════╝
`);
}

const pb = new PocketBase(BASE);
pb.autoCancellation(false);

let recordsOk = false;

try {
  console.log(`Target: ${BASE}\n`);

  console.log('1. PocketBase superuser login...');
  const auth = await pb.collection('_superusers').authWithPassword(email, password);
  const token = pb.authStore.token;
  console.log('   OK — token length:', token?.length);
  console.log('   User:', auth.record?.email);

  console.log('\n2. Fetch chats via fetch (raw + bearer)...');
  for (const style of ['raw', 'bearer']) {
    const r = await fetchRecords(token, style);
    const ok = r.status === 200;
    if (ok) recordsOk = true;
    console.log(`   ${style}: ${ok ? 'OK' : 'FAIL'} (${r.status})`, r.location ? `→ ${r.location}` : '');
    if (!ok) console.log('      ', r.body);
  }

  if (!recordsOk) {
    printBackendDiagnosis();
    process.exitCode = 2;
    return;
  }

  console.log('\n3. Fetch chats via PocketBase SDK...');
  const chats = await pb.collection('chats').getList(1, 5);
  console.log('   OK —', chats.totalItems, 'chats');

  console.log('\n4. Fetch cargo via SDK...');
  const cargo = await pb.collection('cargoListings').getList(1, 5);
  console.log('   OK —', cargo.totalItems, 'cargo listings');

  console.log('\n✅ All admin auth checks passed.');
} catch (err) {
  console.error('\n❌ Failed:', err?.message || err);
  if (err?.status) console.error('   HTTP status:', err.status);
  if (err?.response) console.error('   Response:', JSON.stringify(err.response).slice(0, 200));
  if (!recordsOk) printBackendDiagnosis();
  process.exitCode = 1;
}
