interface AdminModalProps {
  title: string;
  onClose: () => void;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

const AdminModal: React.FC<AdminModalProps> = ({ title, onClose, children, footer }) => (
  <div className="admin-modal-overlay" onClick={onClose} role="presentation">
    <div
      className="admin-modal"
      onClick={(e) => e.stopPropagation()}
      role="dialog"
      aria-modal="true"
      aria-label={title}
    >
      <div className="admin-modal-header">
        <h3>{title}</h3>
        <button type="button" className="admin-modal-close" onClick={onClose} aria-label="Close">
          <i className="ri-close-line" />
        </button>
      </div>
      <div className="admin-modal-body">{children}</div>
      {footer && <div className="admin-modal-footer">{footer}</div>}
    </div>
  </div>
);

export default AdminModal;
