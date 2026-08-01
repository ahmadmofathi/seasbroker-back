import { useEffect } from 'react';
import { useAdminNotifications } from '../../context/AdminNotificationContext';

const AdminNotifications: React.FC = () => {
  const {
    notifications,
    unread,
    loading,
    loadNotifications,
    markRead,
    markAllRead,
    deleteNotification: remove,
  } = useAdminNotifications();

  useEffect(() => {
    loadNotifications();
  }, []);

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
