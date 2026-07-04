import { useEffect, useState } from 'react';
import { cargoApi, quoteApi } from '../../api';
import type { RequestedQuoteRecord } from '../../api/quote';
import { formatApiError } from '../../utils/formatApiError';

function serviceFromNotes(info?: string): string {
  const match = info?.match(/^\[([^\]]+)\]/);
  return match?.[1] ?? 'Quote';
}

const AdminQuotes: React.FC = () => {
  const [quotes, setQuotes] = useState<RequestedQuoteRecord[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [promotingId, setPromotingId] = useState<string | null>(null);

  const load = () => {
    setLoading(true);
    setError('');
    quoteApi
      .listRequestedQuotes()
      .then(setQuotes)
      .catch((e: unknown) => setError(formatApiError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    load();
  }, []);

  const promote = async (quote: RequestedQuoteRecord) => {
    setPromotingId(quote.id);
    setError('');
    try {
      await cargoApi.promoteFromQuote({
        requestedQuoteId: quote.id,
        status: 'Open',
        priority: 3,
      });

      alert('Request promoted to a cargo listing. Open Cargo Listings to see it.');
      load();
    } catch (e) {
      setError(formatApiError(e));
    } finally {
      setPromotingId(null);
    }
  };

  return (
    <>
      {error && (
        <div className="admin-alert-error">
          <i className="ri-error-warning-line" /> {error}
        </div>
      )}

      <div className="admin-action-bar">
        <button type="button" className="admin-btn-sm outline" onClick={load}>
          <i className="ri-refresh-line" /> Refresh
        </button>
        <span className="admin-result-text">
          Public forms (Cargo / Ship / Clearance / Contact) save to the database and appear here.
        </span>
      </div>

      {loading ? (
        <div className="admin-loading">
          <div className="admin-spinner" /> Loading quote requests…
        </div>
      ) : (
        <div className="admin-panel">
          <div className="admin-panel-header">
            <h2>Public Requests ({quotes.length})</h2>
          </div>
          <div className="admin-panel-body no-pad">
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Service</th>
                    <th>Contact</th>
                    <th>Type</th>
                    <th>Route</th>
                    <th>Weight</th>
                    <th>Details</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {quotes.map((q) => (
                    <tr key={q.id}>
                      <td>
                        <span className="admin-badge">{serviceFromNotes(q.additionalInfo)}</span>
                      </td>
                      <td>
                        <div style={{ fontWeight: 500, color: 'var(--admin-navy)' }}>
                          {q.fname} {q.lname}
                        </div>
                        <div style={{ fontSize: '0.75rem', color: 'var(--admin-muted)' }}>
                          {q.email} · {q.phoneNumber}
                        </div>
                      </td>
                      <td>{q.cargoType}</td>
                      <td>
                        {q.departurePort} → {q.arrivalPort}
                      </td>
                      <td>{q.weight} kg</td>
                      <td style={{ maxWidth: 240, fontSize: '0.8rem', color: 'var(--admin-muted)' }}>
                        {q.additionalInfo || q.dimensions}
                      </td>
                      <td>
                        <div className="admin-actions-cell">
                          <button
                            type="button"
                            className="admin-btn-sm primary"
                            disabled={promotingId === q.id}
                            onClick={() => void promote(q)}
                          >
                            {promotingId === q.id ? 'Promoting…' : 'Promote to Cargo'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {quotes.length === 0 && (
                <div className="admin-empty">
                  <i className="ri-file-list-3-line" /> No public requests yet
                  <p style={{ marginTop: '0.75rem', fontSize: '0.85rem' }}>
                    Submit a form on the public site, then refresh this page.
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default AdminQuotes;
