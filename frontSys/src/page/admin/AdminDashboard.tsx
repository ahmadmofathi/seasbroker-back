import { Link } from 'react-router';

const cards = [
  { to: '/admin/quotes', title: 'Public Requests', desc: 'Quotes from website forms', icon: 'ri-file-list-3-line', color: 'red' },
  { to: '/admin/chats', title: 'Chats', desc: 'Customer conversations & replies', icon: 'ri-chat-3-line', color: 'blue' },
  { to: '/admin/cargo', title: 'Cargo', desc: 'Listings lifecycle management', icon: 'ri-ship-line', color: 'navy' },
  { to: '/admin/vessels', title: 'Vessels', desc: 'Fleet & availability windows', icon: 'ri-anchor-line', color: 'green' },
  { to: '/admin/matching', title: 'Matching', desc: 'Engine runs & approvals', icon: 'ri-links-line', color: 'orange' },
  { to: '/admin/notifications', title: 'Notifications', desc: 'Alerts & admin inbox', icon: 'ri-notification-3-line', color: 'purple' },
];

const AdminDashboard: React.FC = () => {
  return (
    <>
      <div className="admin-panel" style={{ marginBottom: '1.5rem' }}>
        <div className="admin-panel-body">
          <h2 style={{ color: 'var(--admin-navy)', margin: '0 0 0.5rem', fontSize: '1.1rem' }}>
            Welcome to SeasBroker Admin
          </h2>
          <p style={{ color: 'var(--admin-muted)', margin: 0, fontSize: '0.9rem' }}>
            Manage chats, cargo listings, vessels, matching, and notifications from one place.
          </p>
        </div>
      </div>

      <div className="admin-stats-grid">
        {cards.map((card) => (
          <Link key={card.to} to={card.to} className="admin-stat-card">
            <div className={`admin-stat-icon ${card.color}`}>
              <i className={card.icon} />
            </div>
            <div>
              <h3>{card.title}</h3>
              <p>{card.desc}</p>
            </div>
          </Link>
        ))}
      </div>
    </>
  );
};

export default AdminDashboard;
