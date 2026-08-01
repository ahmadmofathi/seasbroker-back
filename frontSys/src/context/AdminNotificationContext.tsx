import React, { createContext, useContext, useState, useEffect, useRef } from 'react';
import { notificationsApi, signalrApi } from '../api';
import { getJwtToken, getCollectionToken } from '../api/auth';
import { useAdminAuth } from './AdminAuthContext';
import { useAlert } from './AlertContext';
import type { NotificationRecord } from '../api/types';
import type * as signalR from '@microsoft/signalr';

interface AdminNotificationContextValue {
  notifications: NotificationRecord[];
  unread: NotificationRecord[];
  loading: boolean;
  loadNotifications: () => void;
  markRead: (id: string) => Promise<void>;
  markAllRead: () => Promise<void>;
  deleteNotification: (id: string) => Promise<void>;
}

const AdminNotificationContext = createContext<AdminNotificationContextValue | null>(null);

export const AdminNotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAdminAuth();
  const { success, error: showError, confirm, info } = useAlert();

  const [notifications, setNotifications] = useState<NotificationRecord[]>([]);
  const [unread, setUnread] = useState<NotificationRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const loadNotifications = () => {
    if (!isAuthenticated) return;
    setLoading(true);
    Promise.all([
      notificationsApi.listNotifications(),
      notificationsApi.listUnreadNotifications(),
    ])
      .then(([all, u]) => {
        setNotifications(Array.isArray(all?.items) ? all.items : Array.isArray(all) ? all : []);
        setUnread(Array.isArray(u) ? u : []);
      })
      .catch((e: unknown) => {
        console.error('Failed to load notifications:', e);
      })
      .finally(() => setLoading(false));
  };

  // Load initially when auth status changes
  useEffect(() => {
    if (isAuthenticated) {
      loadNotifications();
    } else {
      setNotifications([]);
      setUnread([]);
    }
  }, [isAuthenticated]);

  // SignalR connection setup
  useEffect(() => {
    if (!isAuthenticated) {
      if (connectionRef.current) {
        void connectionRef.current.stop();
        connectionRef.current = null;
      }
      return;
    }

    const token = getJwtToken() || getCollectionToken();
    if (!token) return;

    const connection = signalrApi.createNotificationsHubConnection(token);
    connectionRef.current = connection;

    // Handle incoming notifications
    signalrApi.onNotificationEvent(connection, (event) => {
      if (event.action === 'create') {
        // Prepend new notification to the lists
        setNotifications((prev) => [event.record, ...prev]);
        if (event.record.status === 'Unread') {
          setUnread((prev) => [event.record, ...prev]);
        }

        // Show a visual toast notification
        info(event.record.message, event.record.title || 'New Admin Notification');
      }
    });

    // Handle reconnect logic to re-join the admin group
    connection.onreconnected(() => {
      signalrApi.joinAdminNotifications(connection).catch((err) => {
        console.error('SignalR re-join admin group failed:', err);
      });
    });

    connection
      .start()
      .then(() => signalrApi.joinAdminNotifications(connection))
      .catch((err) => console.error('Notification hub connect failed', err));

    return () => {
      if (connectionRef.current) {
        void connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [isAuthenticated]);

  const markRead = async (id: string) => {
    try {
      await notificationsApi.markNotificationRead(id);
      loadNotifications();
    } catch {
      showError('Failed to mark notification as read');
    }
  };

  const markAllRead = async () => {
    try {
      const r = await notificationsApi.markAllNotificationsRead();
      success(`Marked ${r.updated} notification(s) as read`);
      loadNotifications();
    } catch {
      showError('Failed to mark all notifications as read');
    }
  };

  const deleteNotification = async (id: string) => {
    const ok = await confirm({
      title: 'Delete notification',
      message: 'Delete this notification?',
      confirmText: 'Delete',
      variant: 'danger',
    });
    if (!ok) return;
    try {
      await notificationsApi.deleteNotification(id);
      loadNotifications();
    } catch {
      showError('Failed to delete notification');
    }
  };

  return (
    <AdminNotificationContext.Provider
      value={{
        notifications,
        unread,
        loading,
        loadNotifications,
        markRead,
        markAllRead,
        deleteNotification,
      }}
    >
      {children}
    </AdminNotificationContext.Provider>
  );
};

export const useAdminNotifications = () => {
  const ctx = useContext(AdminNotificationContext);
  if (!ctx) {
    throw new Error('useAdminNotifications must be used within AdminNotificationProvider');
  }
  return ctx;
};
