import { useState } from 'react';
import { NavLink, Outlet, useLocation, useNavigate, Link } from 'react-router';
import { useAdminAuth } from '../../context/AdminAuthContext';
import { useAdminNotifications } from '../../context/AdminNotificationContext';
import logo from '../../assets/img/Logo_trans.png';
import '../../assets/css/admin.css';

const navItems = [
  { to: '/admin', label: 'Dashboard', icon: 'ri-dashboard-line', end: true },
  { to: '/admin/quotes', label: 'Public Requests', icon: 'ri-file-list-3-line' },
  { to: '/admin/chats', label: 'Chats', icon: 'ri-chat-3-line' },
  { to: '/admin/cargo', label: 'Cargo Listings', icon: 'ri-ship-line' },
  { to: '/admin/vessels', label: 'Vessels', icon: 'ri-anchor-line' },
  { to: '/admin/matching', label: 'Matching', icon: 'ri-links-line' },
  { to: '/admin/form-builder', label: 'Form Builder', icon: 'ri-layout-4-line' },
  { to: '/admin/notifications', label: 'Notifications', icon: 'ri-notification-3-line' },
  { to: '/admin/settings', label: 'System Settings', icon: 'ri-settings-4-line' },
  { to: '/admin/faqs', label: 'FAQs', icon: 'ri-questionnaire-line' },
  { to: '/admin/api-test', label: 'System Health', icon: 'ri-pulse-line' },
];

const pageTitles: Record<string, string> = {
  '/admin': 'Dashboard',
  '/admin/quotes': 'Public Requests',
  '/admin/chats': 'Chats',
  '/admin/cargo': 'Cargo Listings',
  '/admin/vessels': 'Vessels',
  '/admin/matching': 'Matching',
  '/admin/form-builder': 'Form Builder',
  '/admin/notifications': 'Notifications',
  '/admin/settings': 'System Settings',
  '/admin/faqs': 'FAQs',
  '/admin/api-test': 'System Health',
};

const AdminShell: React.FC = () => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const { logout } = useAdminAuth();
  const { unread } = useAdminNotifications();
  const pageTitle = pageTitles[location.pathname] ?? 'Admin';

  const closeSidebar = () => setSidebarOpen(false);

  const handleLogout = () => {
    logout();
    void navigate('/admin/login');
  };

  return (
    <div className="admin-app">
      <div
        className={`admin-overlay${sidebarOpen ? ' open' : ''}`}
        onClick={closeSidebar}
        role="presentation"
      />

      <aside className={`admin-sidebar${sidebarOpen ? ' open' : ''}`}>
        <div className="admin-sidebar-header">
          <img src={logo} alt="Seas Broker" />
          <div className="brand-text">
            <h3>Seasbroker</h3>
            <span>Admin Panel</span>
          </div>
        </div>

        <nav className="admin-nav">
          {navItems.map((item) => {
            const isNotifications = item.to === '/admin/notifications';
            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                onClick={closeSidebar}
                className={({ isActive }) => `admin-nav-link${isActive ? ' active' : ''}`}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                  <i className={item.icon} />
                  <span>{item.label}</span>
                </div>
                {isNotifications && unread.length > 0 && (
                  <span className="admin-nav-badge">{unread.length}</span>
                )}
              </NavLink>
            );
          })}
        </nav>

        <div className="admin-sidebar-footer">
          <Link to="/" onClick={closeSidebar}>
            <i className="ri-home-4-line" />
            Public Website
          </Link>
          <button type="button" className="logout-btn" onClick={handleLogout}>
            <i className="ri-logout-box-r-line" />
            Logout
          </button>
        </div>
      </aside>

      <div className="admin-main">
        <header className="admin-topbar">
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <button
              type="button"
              className="admin-menu-toggle"
              onClick={() => setSidebarOpen(true)}
              aria-label="Open menu"
            >
              <i className="ri-menu-line" />
            </button>
            <div className="admin-topbar-title">
              <h1>{pageTitle}</h1>
              <p>SeasBroker Management Console</p>
            </div>
          </div>
          <div className="admin-topbar-actions">
            <span className="admin-api-badge">
              <span className="dot" />
              Connected
            </span>
          </div>
        </header>

        <div className="admin-content">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default AdminShell;
