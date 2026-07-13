import { useEffect, useState } from 'react';
import { notificationsApi } from '../../api';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import type { NotificationRecord } from '../../api/types';

const AdminNotifications: React.FC = () => {
  const [notifications, setNotifications] = useState<NotificationRecord[]>([]);
  const [unread, setUnread] = useState<NotificationRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const { success, error: showError, confirm } = useAlert();

  const load = () => {
    setLoading(true);
    Promise.all([
      notificationsApi.listNotifications(),
      notificationsApi.listUnreadNotifications(),
    ])
      .then(([all, u]) => {
        setNotifications(Array.isArray(all?.items) ? all.items : Array.isArray(all) ? all : []);
        setUnread(Array.isArray(u) ? u : []);
      })
      .catch((e: unknown) => showError(formatApiError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const markRead = async (id: string) => {
    try {
      await notificationsApi.markNotificationRead(id);
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const markAllRead = async () => {
    try {
      const r = await notificationsApi.markAllNotificationsRead();
      success(`Marked ${r.updated} notification(s) as read`);
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const remove = async (id: string) => {
    const ok = await confirm({
      title: 'Delete notification',
      message: 'Delete this notification?',
      confirmText: 'Delete',
      variant: 'danger',
    });
    if (!ok) return;
    try {
      await notificationsApi.deleteNotification(id);
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  return (
    <>
      <div className="admin-action-bar">
        <span className="admin-unread-pill">{unread.length} unread</span>
        <button type="button" className="admin-btn-sm outline" onClick={() => void markAllRead()}>
          Mark all as read
        </button>
      </div>

      {loading ? (
        <div className="admin-loading"><div className="admin-spinner" /> Loading notifications…</div>
      ) : (
        <div className="admin-panel">
          <div className="admin-panel-header">
            <h2>All Notifications ({notifications.length})</h2>
          </div>
          <div className="admin-panel-body no-pad">
            {notifications.map((n) => (
              <div key={n.id} className={`admin-notif-item${n.status === 'Unread' ? ' unread' : ''}`}>
                <div>
                  <h4>{n.title}</h4>
                  <p>{n.message}</p>
                  <div className="admin-notif-meta">{n.notificationType} · {new Date(n.createdAt).toLocaleString()}</div>
                </div>
                <div className="admin-actions-cell">
                  {n.status === 'Unread' && (
                    <button type="button" className="admin-btn-sm outline" onClick={() => void markRead(n.id)}>
                      Mark read
                    </button>
                  )}
                  <button type="button" className="admin-btn-sm danger" onClick={() => void remove(n.id)}>
                    Delete
                  </button>
                </div>
              </div>
            ))}
            {notifications.length === 0 && (
              <div className="admin-empty"><i className="ri-notification-off-line" /> No notifications</div>
            )}
          </div>
        </div>
      )}
    </>
  );
};

export default AdminNotifications;
