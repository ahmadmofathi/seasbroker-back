import React, { useEffect, useState } from 'react';
import { listCollection, createRecord, updateRecord, deleteRecord } from '../../api/client';
import { useAlert } from '../../context/AlertContext';
import { formatApiError } from '../../utils/formatApiError';

interface FaqRecord {
  id: string;
  heading: string;
  para: string;
  sortOrder: number;
}

const AdminFaqs: React.FC = () => {
  const [faqs, setFaqs] = useState<FaqRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingFaq, setEditingFaq] = useState<FaqRecord | null>(null);

  // Form states
  const [heading, setHeading] = useState('');
  const [para, setPara] = useState('');
  const [sortOrder, setSortOrder] = useState(1);
  const [submitting, setSubmitting] = useState(false);

  const { success, error: showError } = useAlert();

  const load = () => {
    setLoading(true);
    listCollection<FaqRecord>('faqs', { page: 1, perPage: 100 })
      .then((res) => {
        setFaqs(res.items || []);
      })
      .catch((e: unknown) => showError(formatApiError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    load();
  }, []);

  const openAddModal = () => {
    setEditingFaq(null);
    setHeading('');
    setPara('');
    setSortOrder(faqs.length + 1);
    setModalOpen(true);
  };

  const openEditModal = (faq: FaqRecord) => {
    setEditingFaq(faq);
    setHeading(faq.heading);
    setPara(faq.para);
    setSortOrder(faq.sortOrder);
    setModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);

    try {
      const payload = { heading, para, sortOrder: Number(sortOrder) };

      if (editingFaq) {
        await updateRecord('faqs', editingFaq.id, payload);
        success('FAQ updated successfully.');
      } else {
        await createRecord('faqs', payload);
        success('FAQ created successfully.');
      }

      setModalOpen(false);
      load();
    } catch (err: unknown) {
      showError(formatApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (faq: FaqRecord) => {
    if (!window.confirm(`Are you sure you want to delete "${faq.heading}"?`)) return;

    try {
      await deleteRecord('faqs', faq.id);
      success('FAQ deleted successfully.');
      load();
    } catch (err: unknown) {
      showError(formatApiError(err));
    }
  };

  return (
    <>
      <div className="admin-action-bar" style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
        <button type="button" className="admin-btn-sm primary" onClick={openAddModal}>
          <i className="ri-add-line" /> Add FAQ
        </button>
        <button type="button" className="admin-btn-sm outline" onClick={load}>
          <i className="ri-refresh-line" /> Refresh
        </button>
      </div>

      {loading ? (
        <div className="admin-panel" style={{ padding: '2rem', textAlign: 'center' }}>
          <p style={{ color: 'var(--admin-muted)' }}>Loading FAQs...</p>
        </div>
      ) : faqs.length === 0 ? (
        <div className="admin-panel" style={{ padding: '2rem', textAlign: 'center' }}>
          <p style={{ color: 'var(--admin-muted)' }}>No FAQs registered yet. Click "Add FAQ" to start.</p>
        </div>
      ) : (
        <div className="admin-table-container" style={{ overflowX: 'auto', background: '#fff', borderRadius: 'var(--admin-radius)', border: '1px solid var(--admin-border)' }}>
          <table className="admin-table" style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
            <thead>
              <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--admin-border)' }}>
                <th style={{ padding: '0.85rem 1rem' }}>Order</th>
                <th style={{ padding: '0.85rem 1rem' }}>Question / Heading</th>
                <th style={{ padding: '0.85rem 1rem' }}>Answer / Paragraph</th>
                <th style={{ padding: '0.85rem 1rem', textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {faqs.map((faq) => (
                <tr key={faq.id} style={{ borderBottom: '1px solid var(--admin-border)' }}>
                  <td style={{ padding: '0.85rem 1rem', width: '80px', fontWeight: 600 }}>#{faq.sortOrder}</td>
                  <td style={{ padding: '0.85rem 1rem', width: '300px', fontWeight: 500, color: 'var(--admin-navy)' }}>{faq.heading}</td>
                  <td style={{ padding: '0.85rem 1rem', color: 'var(--admin-muted)', whiteSpace: 'normal', maxWidth: '500px' }}>
                    {faq.para.length > 150 ? `${faq.para.substring(0, 150)}...` : faq.para}
                  </td>
                  <td style={{ padding: '0.85rem 1rem', textAlign: 'right' }}>
                    <div className="admin-actions-cell" style={{ display: 'inline-flex', gap: '0.5rem' }}>
                      <button type="button" className="admin-btn-sm outline" onClick={() => openEditModal(faq)}>
                        Edit
                      </button>
                      <button type="button" className="admin-btn-sm danger" onClick={() => handleDelete(faq)} style={{ color: '#ef4444', borderColor: '#fca5a5', background: 'transparent' }}>
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {modalOpen && (
        <div className="admin-modal-overlay">
          <div className="admin-modal" style={{ background: '#fff', borderRadius: 'var(--admin-radius)', width: 'min(600px, 100%)', overflow: 'hidden' }}>
            <div className="admin-modal-header" style={{ display: 'flex', justifyContent: 'space-between', padding: '1rem 1.25rem', borderBottom: '1px solid var(--admin-border)' }}>
              <h3 style={{ margin: 0 }}>{editingFaq ? 'Edit FAQ' : 'Add FAQ'}</h3>
              <button type="button" className="admin-modal-close" onClick={() => setModalOpen(false)}>×</button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="admin-modal-body" style={{ padding: '1.25rem' }}>
                <div className="admin-form-grid">
                  <div className="admin-field full">
                    <label>Question / Heading</label>
                    <input
                      type="text"
                      className="admin-input"
                      value={heading}
                      onChange={(e) => setHeading(e.target.value)}
                      placeholder="e.g. TRANSPORT & LOGISTIC SERVICES"
                      required
                    />
                  </div>
                  <div className="admin-field full" style={{ marginTop: '1rem' }}>
                    <label>Answer / Paragraph</label>
                    <textarea
                      className="admin-input"
                      rows={6}
                      value={para}
                      onChange={(e) => setPara(e.target.value)}
                      placeholder="Enter the FAQ answer text here..."
                      required
                    />
                  </div>
                  <div className="admin-field" style={{ marginTop: '1rem' }}>
                    <label>Sort Order</label>
                    <input
                      type="number"
                      className="admin-input"
                      value={sortOrder}
                      onChange={(e) => setSortOrder(Number(e.target.value))}
                      required
                    />
                  </div>
                </div>
              </div>
              <div className="admin-modal-footer" style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', padding: '0.85rem 1.25rem 1.15rem', borderTop: '1px solid var(--admin-border)' }}>
                <button type="button" className="admin-btn-sm outline" onClick={() => setModalOpen(false)}>
                  Cancel
                </button>
                <button type="submit" className="admin-btn-sm primary" disabled={submitting}>
                  {submitting ? 'Saving...' : 'Save'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </>
  );
};

export default AdminFaqs;
