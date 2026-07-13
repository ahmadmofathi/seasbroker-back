import { useEffect, useState } from 'react';
import { cargoApi, matchingApi, vesselsApi } from '../../api';
import AdminModal from '../../component/admin/AdminModal';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import type { CargoListingRecord, MatchRecord, VesselRecord } from '../../api/types';

function toMatchList(value: unknown): MatchRecord[] {
  return Array.isArray(value) ? value : [];
}

const MatchTable = ({
  title,
  data,
  actions,
  onApprove,
  onReject,
}: {
  title: string;
  data: MatchRecord[];
  actions?: boolean;
  onApprove?: (id: string) => void;
  onReject?: (id: string) => void;
}) => {
  const rows = toMatchList(data);

  return (
    <div className="admin-panel">
      <div className="admin-panel-header">
        <h2>
          {title} ({rows.length})
        </h2>
      </div>
      <div className="admin-panel-body no-pad">
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Score</th>
                <th>Status</th>
                <th>Source</th>
                <th>Cargo</th>
                <th>Vessel</th>
                {actions && <th>Actions</th>}
              </tr>
            </thead>
            <tbody>
              {rows.map((m) => (
                <tr key={m.id}>
                  <td style={{ fontWeight: 600, color: 'var(--admin-navy)' }}>{m.score}</td>
                  <td>
                    <span className="admin-badge">{m.status}</span>
                  </td>
                  <td>{m.source}</td>
                  <td>{m.cargoListingId?.slice(0, 8)}</td>
                  <td>{m.vesselId?.slice(0, 8)}</td>
                  {actions && (
                    <td>
                      <div className="admin-actions-cell">
                        <button
                          type="button"
                          className="admin-btn-sm success"
                          onClick={() => onApprove?.(m.id)}
                        >
                          Approve
                        </button>
                        <button
                          type="button"
                          className="admin-btn-sm danger"
                          onClick={() => onReject?.(m.id)}
                        >
                          Reject
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
          {rows.length === 0 && (
            <div className="admin-empty">
              <p>No matches</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

const AdminMatching: React.FC = () => {
  const [pending, setPending] = useState<MatchRecord[]>([]);
  const [approved, setApproved] = useState<MatchRecord[]>([]);
  const [all, setAll] = useState<MatchRecord[]>([]);
  const [cargoOptions, setCargoOptions] = useState<CargoListingRecord[]>([]);
  const [vesselOptions, setVesselOptions] = useState<VesselRecord[]>([]);
  const [runResult, setRunResult] = useState('');
  const [loading, setLoading] = useState(true);
  const [openManual, setOpenManual] = useState(false);
  const [saving, setSaving] = useState(false);
  const { error: showError } = useAlert();
  const [manualForm, setManualForm] = useState({
    cargoListingId: '',
    vesselId: '',
    score: '80',
    matchReason: '',
  });

  const load = () => {
    setLoading(true);

    Promise.allSettled([
      matchingApi.listPendingApproval(),
      matchingApi.listApprovedMatches(),
      matchingApi.listMatches(),
      cargoApi.listCargoListings(),
      vesselsApi.listVessels(),
    ])
      .then((results) => {
        const [pendingResult, approvedResult, allResult, cargoResult, vesselResult] = results;

        setPending(pendingResult.status === 'fulfilled' ? toMatchList(pendingResult.value) : []);
        setApproved(approvedResult.status === 'fulfilled' ? toMatchList(approvedResult.value) : []);
        setAll(allResult.status === 'fulfilled' ? toMatchList(allResult.value) : []);
        setCargoOptions(cargoResult.status === 'fulfilled' ? cargoResult.value : []);
        setVesselOptions(vesselResult.status === 'fulfilled' ? vesselResult.value : []);

        const firstError = results.find((r) => r.status === 'rejected') as
          | PromiseRejectedResult
          | undefined;
        if (firstError) {
          showError(formatApiError(firstError.reason));
        }
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    load();
  }, []);

  const runMatching = async () => {
    try {
      const r = await matchingApi.runMatching({});
      setRunResult(`Created: ${r.matchesCreated} · Skipped: ${r.matchesSkipped}`);
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const approve = async (id: string) => {
    try {
      await matchingApi.approveMatch(id, { reason: 'Approved via admin dashboard' });
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const reject = async (id: string) => {
    try {
      await matchingApi.rejectMatch(id, { reason: 'Rejected via admin dashboard' });
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const createManual = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await matchingApi.createManualMatch({
        cargoListingId: manualForm.cargoListingId,
        vesselId: manualForm.vesselId,
        score: Number(manualForm.score) || 0,
        matchReason: manualForm.matchReason.trim(),
      });
      setOpenManual(false);
      setManualForm({ cargoListingId: '', vesselId: '', score: '80', matchReason: '' });
      load();
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <>
      <div className="admin-action-bar">
        <button type="button" className="admin-btn-sm primary" onClick={() => void runMatching()}>
          <i className="ri-play-circle-line" /> Run Matching Engine
        </button>
        <button type="button" className="admin-btn-sm outline" onClick={() => setOpenManual(true)}>
          <i className="ri-add-line" /> Manual Match
        </button>
        {runResult && <span className="admin-result-text">{runResult}</span>}
      </div>

      {loading ? (
        <div className="admin-loading">
          <div className="admin-spinner" /> Loading matches…
        </div>
      ) : (
        <>
          <MatchTable
            title="Pending Approval"
            data={pending}
            actions
            onApprove={(id) => void approve(id)}
            onReject={(id) => void reject(id)}
          />
          <MatchTable title="Approved" data={approved} />
          <MatchTable title="All Matches" data={all} />
        </>
      )}

      {openManual && (
        <AdminModal
          title="Create Manual Match"
          onClose={() => !saving && setOpenManual(false)}
          footer={
            <>
              <button
                type="button"
                className="admin-btn-sm outline"
                onClick={() => setOpenManual(false)}
                disabled={saving}
              >
                Cancel
              </button>
              <button type="submit" form="manual-match-form" className="admin-btn-sm primary" disabled={saving}>
                {saving ? 'Creating…' : 'Create Match'}
              </button>
            </>
          }
        >
          <form id="manual-match-form" className="admin-form-grid" onSubmit={(e) => void createManual(e)}>
            <div className="admin-field full">
              <label htmlFor="m-cargo">Cargo Listing</label>
              <select
                id="m-cargo"
                className="admin-input"
                required
                value={manualForm.cargoListingId}
                onChange={(e) => setManualForm((p) => ({ ...p, cargoListingId: e.target.value }))}
              >
                <option value="">Select cargo…</option>
                {cargoOptions.map((c) => (
                  <option key={c.id} value={c.id}>
                    {(c.referenceNumber || c.id.slice(0, 8))} — {c.cargoType} ({c.departurePort} → {c.arrivalPort})
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field full">
              <label htmlFor="m-vessel">Vessel</label>
              <select
                id="m-vessel"
                className="admin-input"
                required
                value={manualForm.vesselId}
                onChange={(e) => setManualForm((p) => ({ ...p, vesselId: e.target.value }))}
              >
                <option value="">Select vessel…</option>
                {vesselOptions.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.name} ({v.imoNumber})
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field">
              <label htmlFor="m-score">Score</label>
              <input
                id="m-score"
                className="admin-input"
                type="number"
                min="0"
                max="100"
                required
                value={manualForm.score}
                onChange={(e) => setManualForm((p) => ({ ...p, score: e.target.value }))}
              />
            </div>
            <div className="admin-field full">
              <label htmlFor="m-reason">Match Reason</label>
              <input
                id="m-reason"
                className="admin-input"
                required
                value={manualForm.matchReason}
                onChange={(e) => setManualForm((p) => ({ ...p, matchReason: e.target.value }))}
                placeholder="Why this match?"
              />
            </div>
          </form>
        </AdminModal>
      )}
    </>
  );
};

export default AdminMatching;
