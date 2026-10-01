import { useEffect, useState } from 'react';
import { Link } from 'react-router';
import { cargoApi, quoteApi, vesselsApi } from '../../api';
import type { RequestAttachment, RequestedQuoteRecord } from '../../api/quote';
import type { PromoteFromQuoteBody } from '../../api/types';
import PromoteCargoModal from '../../component/admin/PromoteCargoModal';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';

function formatSize(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  return `${String(Math.max(1, Math.round(bytes / 1024)))} KB`;
}

function serviceFromNotes(info?: string): string {
  const match = info?.match(/^\[([^\]]+)\]/);
  return match?.[1] ?? 'Quote';
}

const AdminQuotes: React.FC = () => {
  const [quotes, setQuotes] = useState<RequestedQuoteRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [promotingId, setPromotingId] = useState<string | null>(null);
  const { success, error: showError } = useAlert();

  const load = () => {
    setLoading(true);
    quoteApi
      .listRequestedQuotes()
      .then(setQuotes)
      .catch((e: unknown) => showError(formatApiError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    load();
  }, []);

  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  const download = async (attachment: RequestAttachment) => {
    setDownloadingId(attachment.id);
    try {
      await quoteApi.downloadAttachment(attachment);
    } catch (e) {
      showError(formatApiError(e));
    } finally {
      setDownloadingId(null);
    }
  };

  // The request being reviewed in the promote dialog, if it's open.
  const [reviewing, setReviewing] = useState<RequestedQuoteRecord | null>(null);

  const promote = async (body: PromoteFromQuoteBody) => {
    setPromotingId(body.requestedQuoteId);
    try {
      await cargoApi.promoteFromQuote(body);

      success('Request promoted to a cargo listing. Open Cargo Listings to see it.');
      setReviewing(null);
      load();
    } catch (e) {
      showError(formatApiError(e));
    } finally {
      setPromotingId(null);
    }
  };

  const promoteToVessel = async (quote: RequestedQuoteRecord) => {
    setPromotingId(quote.id);
    try {
      const result = await vesselsApi.promoteFromQuote(quote.id);
      const notes = [
        `"${result.vessel.name}" added to the fleet.`,
        result.availability
          ? 'Its availability window was created, so matching can use it.'
          : result.availabilityNote ?? '',
        result.cancelledCargoListingReference
          ? `The cargo listing ${result.cancelledCargoListingReference} made from this request by mistake was cancelled.`
          : '',
      ];
      success(notes.filter(Boolean).join(' '));
      load();
    } catch (e) {
      showError(formatApiError(e));
    } finally {
      setPromotingId(null);
    }
  };

  return (
    <>
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
                        {q.trackingNumber && (
                          <div style={{ fontSize: '0.75rem', color: 'var(--admin-muted)' }} title="The customer's tracking number">
                            Tracking: <strong>{q.trackingNumber}</strong>
                          </div>
                        )}
                      </td>
                      <td>{q.cargoType}</td>
                      <td>
                        {q.departurePort} → {q.arrivalPort}
                      </td>
                      <td>{q.weight.toLocaleString()} MT</td>
                      <td style={{ maxWidth: 240, fontSize: '0.8rem', color: 'var(--admin-muted)' }}>
                        {q.additionalInfo || q.dimensions}
                        {q.attachments && q.attachments.length > 0 && (
                          <div style={{ marginTop: '0.5rem' }}>
                            <div style={{ fontWeight: 600, color: 'var(--admin-navy)' }}>
                              Attachments ({q.attachments.length})
                            </div>
                            {q.attachments.map((file) => (
                              <button
                                key={file.id}
                                type="button"
                                className="admin-btn-sm outline"
                                style={{ display: 'block', marginTop: '0.25rem', maxWidth: '100%', textAlign: 'start', overflowWrap: 'anywhere' }}
                                title={`Download ${file.fileName} (uploaded to "${file.fieldKey}")`}
                                disabled={downloadingId === file.id}
                                onClick={() => void download(file)}
                              >
                                <i className="ri-attachment-2" />{' '}
                                {downloadingId === file.id ? 'Downloading…' : `${file.fileName} · ${formatSize(file.sizeBytes)}`}
                              </button>
                            ))}
                          </div>
                        )}
                      </td>
                      <td>
                        <div className="admin-actions-cell">
                          {q.vesselId ? (
                            <Link
                              className="admin-badge"
                              to={`/admin/vessels?vessel=${q.vesselId}`}
                              title="Open this vessel in the fleet"
                            >
                              <i className="ri-ship-line" /> In fleet
                              {q.vesselName ? ` · ${q.vesselName}` : ''}
                            </Link>
                          ) : q.canPromoteToVessel ? (
                            <button
                              type="button"
                              className="admin-btn-sm primary"
                              disabled={promotingId === q.id}
                              title={q.cargoListingId ? 'This ship request was promoted to cargo by mistake - this adds it to the fleet and cancels that listing' : undefined}
                              onClick={() => void promoteToVessel(q)}
                            >
                              {promotingId === q.id ? 'Adding…' : 'Promote to Vessel'}
                            </button>
                          ) : q.isPromoted ? (
                            <Link
                              className="admin-badge"
                              to={q.cargoListingId ? `/admin/cargo?listing=${q.cargoListingId}` : '/admin/cargo'}
                              title="Open this request's cargo listing"
                            >
                              <i className="ri-checkbox-circle-line" /> Promoted
                              {q.cargoListingReference ? ` · ${q.cargoListingReference}` : ''}
                            </Link>
                          ) : !q.canPromote && serviceFromNotes(q.additionalInfo) === 'Ship Brokerage' ? (
                            // Sent before the Forms module: the vessel details are only free text, so it
                            // can't be added to the fleet automatically.
                            <Link
                              className="admin-result-text"
                              to="/admin/vessels"
                              title="This older ship request can't be added automatically - add the vessel by hand"
                            >
                              Add vessel manually
                            </Link>
                          ) : !q.canPromote ? (
                            <span className="admin-result-text" title="Only Cargo Brokerage requests can become cargo listings">
                              Not cargo
                            </span>
                          ) : (
                            <button
                              type="button"
                              className="admin-btn-sm primary"
                              disabled={promotingId === q.id}
                              onClick={() => { setReviewing(q); }}
                            >
                              {promotingId === q.id ? 'Promoting…' : 'Promote to Cargo'}
                            </button>
                          )}
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
      {reviewing && (
        <PromoteCargoModal
          quote={reviewing}
          saving={promotingId === reviewing.id}
          onClose={() => { setReviewing(null); }}
          onPromote={promote}
        />
      )}
    </>
  );
};

export default AdminQuotes;
