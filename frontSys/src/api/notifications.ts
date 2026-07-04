import { adminRequest } from './adminClient';
import type { NotificationRecord, PaginatedResponse } from './types';

export async function listNotifications(
  page = 1,
  perPage = 50,
): Promise<PaginatedResponse<NotificationRecord>> {
  return adminRequest<PaginatedResponse<NotificationRecord>>('/api/notifications', {
    query: { page, perPage },
  });
}

export async function listUnreadNotifications(): Promise<NotificationRecord[]> {
  return adminRequest<NotificationRecord[]>('/api/notifications/unread');
}

export async function markNotificationRead(id: string): Promise<void> {
  return adminRequest<void>(`/api/notifications/${id}/read`, { method: 'POST' });
}

export async function markAllNotificationsRead(): Promise<{ updated: number }> {
  return adminRequest<{ updated: number }>('/api/notifications/read-all', { method: 'POST' });
}

export async function deleteNotification(id: string): Promise<void> {
  return adminRequest<void>(`/api/notifications/${id}`, { method: 'DELETE' });
}
