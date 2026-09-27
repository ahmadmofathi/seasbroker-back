import { useEffect, useMemo, useState } from 'react';
import { cargoApi, matchingApi, vesselsApi } from '../../api';
import AdminModal from '../../component/admin/AdminModal';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import type { CargoListingRecord, MatchingRuleRecord, MatchRecord, VesselRecord } from '../../api/types';

function toMatchList(value: unknown): MatchRecord[] {
  return Array.isArray(value) ? value : [];
}

const muted: React.CSSProperties = { fontSize: '0.75rem', color: 'var(--admin-muted)' };

const MatchTable = ({
  title,
  data,
  cargoById,
  vesselById,
  renderActions,
}: {
  title: string;
  data: MatchRecord[];
  cargoById: Map<string, CargoListingRecord>;
  vesselById: Map<string, VesselRecord>;
  renderActions?: (m: MatchRecord) => React.ReactNode;
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
                {renderActions && <th>Actions</th>}
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
                  <td>
                    <CargoCell match={m} cargo={cargoById.get(m.cargoListingId)} />
                  </td>
                  <td>
                    <VesselCell match={m} vessel={vesselById.get(m.vesselId)} />
                  </td>
                  {renderActions && (
                    <td>
                      <div className="admin-actions-cell">{renderActions(m)}</div>
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

/**
 * Reference and cargo type, with route and customer underneath - instead of a bare id. Uses the
 * details the API sends with the match, then the loaded cargo list, and only then the id.
 */
const CargoCell = ({ match, cargo }: { match: MatchRecord; cargo?: CargoListingRecord }) => {
  const reference = match.cargoReference ?? cargo?.referenceNumber;
  const type = match.cargoType ?? cargo?.cargoType;
  const route = match.cargoRoute ?? (cargo ? `${cargo.departurePort} → ${cargo.arrivalPort}` : null);
  const customer = match.customerName ?? cargo?.customerName;
  if (!reference && !type) return <span title={match.cargoListingId}>{match.cargoListingId.slice(0, 8)}</span>;
  return (
    <div title={match.cargoListingId}>
      <div style={{ fontWeight: 500, color: 'var(--admin-navy)' }}>
        {reference ?? match.cargoListingId.slice(0, 8)}
        {type ? ` · ${type}` : ''}
      </div>
      <div style={muted}>
        {route}
        {customer ? ` · ${customer}` : ''}
      </div>
    </div>
  );
};

const VesselCell = ({ match, vessel }: { match: MatchRecord; vessel?: VesselRecord }) => {
  const name = match.vesselName ?? vessel?.name;
  const type = match.vesselType ?? vessel?.vesselType;
  const dwt = match.vesselDwt ?? vessel?.dwt;
  const imo = match.vesselImo ?? vessel?.imoNumber;
  if (!name) return <span title={match.vesselId}>{match.vesselId.slice(0, 8)}</span>;
  return (
    <div title={match.vesselId}>
      <div style={{ fontWeight: 500, color: 'var(--admin-navy)' }}>{name}</div>
      <div style={muted}>
        {[type, dwt != null ? `${dwt.toLocaleString()} MT DWT` : null, imo ? `IMO ${imo}` : null]
          .filter(Boolean)
          .join(' · ')}
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
  const cargoById = useMemo(() => new Map(cargoOptions.map((c) => [c.id, c])), [cargoOptions]);
  const vesselById = useMemo(() => new Map(vesselOptions.map((v) => [v.id, v])), [vesselOptions]);
  const [rules, setRules] = useState<MatchingRuleRecord[]>([]);
  const [ruleWeights, setRuleWeights] = useState<Record<string, string>>({});
  const [savingRuleId, setSavingRuleId] = useState<string | null>(null);
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
      cargoApi.listCargoListings(undefined, 1, 200),
      vesselsApi.listVessels(undefined, 1, 200),
      matchingApi.listMatchingRules(),
    ])
      .then((results) => {
        const [pendingResult, approvedResult, allResult, cargoResult, vesselResult, rulesResult] = results;

        setPending(pendingResult.status === 'fulfilled' ? toMatchList(pendingResult.value) : []);
        setApproved(approvedResult.status === 'fulfilled' ? toMatchList(approvedResult.value) : []);
        setAll(allResult.status === 'fulfilled' ? toMatchList(allResult.value) : []);
        setCargoOptions(cargoResult.status === 'fulfilled' ? cargoResult.value : []);
        setVesselOptions(vesselResult.status === 'fulfilled' ? vesselResult.value : []);

        const ruleList = rulesResult.status === 'fulfilled' ? rulesResult.value : [];
        setRules(ruleList);
        setRuleWeights(Object.fromEntries(ruleList.map((r) => [r.id, String(r.weight)])));

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

  const expire = async (id: string) => {
    try {
      await matchingApi.expireMatch(id);
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const cancel = async (id: string) => {
    try {
      await matchingApi.cancelMatch(id, { reason: 'Cancelled via admin dashboard' });
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const complete = async (id: string) => {
    try {
      await matchingApi.completeMatch(id, { reason: 'Completed via admin dashboard' });
      load();
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  const saveRuleWeight = async (rule: MatchingRuleRecord) => {
    const weight = Number(ruleWeights[rule.id]);
    if (Number.isNaN(weight)) {
      showError('Weight must be a number.');
      return;
    }
    setSavingRuleId(rule.id);
    try {
      await matchingApi.updateMatchingRule(rule.id, { weight });
      load();
    } catch (e) {
      showError(formatApiError(e));
    } finally {
      setSavingRuleId(null);
    }
  };

  const toggleRuleActive = async (rule: MatchingRuleRecord) => {
    setSavingRuleId(rule.id);
    try {
      await matchingApi.updateMatchingRule(rule.id, { isActive: !rule.isActive });
      load();
    } catch (e) {
      showError(formatApiError(e));
    } finally {
      setSavingRuleId(null);
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
          <div className="admin-panel">
            <div className="admin-panel-header">
              <h2>Matching Rules ({rules.length})</h2>
            </div>
            <div className="admin-panel-body no-pad">
              <div className="admin-table-wrap">
                <table className="admin-table">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Criterion</th>
                      <th>Weight</th>
                      <th>Active</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {rules.map((rule) => (
                      <tr key={rule.id}>
                        <td style={{ fontWeight: 500, color: 'var(--admin-navy)' }}>{rule.name}</td>
                        <td>{rule.criterion}</td>
                        <td>
                          <input
                            type="number"
                            step="0.01"
                            className="admin-input"
                            style={{ width: 90 }}
                            value={ruleWeights[rule.id] ?? ''}
                            onChange={(e) =>
                              setRuleWeights((prev) => ({ ...prev, [rule.id]: e.target.value }))
                            }
                          />
                        </td>
                        <td>
                          <input
                            type="checkbox"
                            checked={rule.isActive}
                            onChange={() => void toggleRuleActive(rule)}
                          />
                        </td>
                        <td>
                          <button
                            type="button"
                            className="admin-btn-sm outline"
                            disabled={savingRuleId === rule.id}
                            onClick={() => void saveRuleWeight(rule)}
                          >
                            {savingRuleId === rule.id ? 'Saving…' : 'Save Weight'}
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {rules.length === 0 && (
                  <div className="admin-empty">
                    <p>No matching rules configured</p>
                  </div>
                )}
              </div>
            </div>
          </div>

          <MatchTable
            cargoById={cargoById}
            vesselById={vesselById}
            title="Pending Approval"
            data={pending}
            renderActions={(m) => (
              <>
                <button type="button" className="admin-btn-sm success" onClick={() => void approve(m.id)}>
                  Approve
                </button>
                <button type="button" className="admin-btn-sm danger" onClick={() => void reject(m.id)}>
                  Reject
                </button>
                <button type="button" className="admin-btn-sm outline" onClick={() => void expire(m.id)}>
                  Expire
                </button>
              </>
            )}
          />
          <MatchTable
            cargoById={cargoById}
            vesselById={vesselById}
            title="Approved"
            data={approved}
            renderActions={(m) => (
              <>
                <button type="button" className="admin-btn-sm success" onClick={() => void complete(m.id)}>
                  Complete
                </button>
                <button type="button" className="admin-btn-sm danger" onClick={() => void cancel(m.id)}>
                  Cancel
                </button>
              </>
            )}
          />
          <MatchTable title="All Matches" data={all} cargoById={cargoById} vesselById={vesselById} />
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
