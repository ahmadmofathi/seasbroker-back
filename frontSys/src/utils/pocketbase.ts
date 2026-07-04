import PocketBase from 'pocketbase';
import { getAuthMode } from '../api/authTokenStore';
import { resolveApiOrigin } from '../api/client';

const pb = new PocketBase(resolveApiOrigin());

// React Strict Mode cancels duplicate in-flight SDK requests (shows as "canceled" in Network).
pb.autoCancellation(false);

pb.beforeSend = (url, options) => {
  const token = pb.authStore.token;
  if (!token || url.includes('auth-with-password')) {
    return { url, options };
  }

  const style = getAuthMode();
  const headers = { ...(options.headers as Record<string, string>) };
  headers.Authorization = style === 'bearer' ? `Bearer ${token}` : token;
  options.headers = headers;

  return { url, options };
};

export default pb;
