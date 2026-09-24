import { useEffect, useState } from 'react';
import { vesselsApi } from '../../api';
import type { RouteStopValue, VesselAvailabilityRecord, VesselRecord } from '../../api/types';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import { isKnownPort } from '../../utils/portOptions';
import { stopLabel, validateRoute } from '../../utils/route';
import RouteBuilder from '../Common/RouteBuilder';
import AdminModal from './AdminModal';

type AvailabilityForm = {
  availableFrom: string;
  availableTo: string;
  route: RouteStopValue[];
};

function describeRoute(item: VesselAvailabilityRecord): string {
  if (item.routeStops && item.routeStops.length > 0) {
    return item.routeStops
      .map((s) => `${s.port} (${new Date(s.eta).toLocaleDateString()})`)
      .join(' → ');
  }
  return [item.openPort, item.destinationPort].filter(Boolean).join(' → ');
}

function toLocalInput(value?: string): string {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function defaultAvailabilityForm(vessel: VesselRecord): AvailabilityForm {
  const from = new Date();
  const to = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000);
  const port = isKnownPort(vessel.currentPort) ? vessel.currentPort.trim() : '';
  return {
    availableFrom: toLocalInput(from.toISOString()),
    availableTo: toLocalInput(to.toISOString()),
    route: [{ port, eta: '' }],
  };
}

interface VesselAvailabilityModalProps {
  vessel: VesselRecord;
  onClose: () => void;
}

const VesselAvailabilityModal: React.FC<VesselAvailabilityModalProps> = ({ vessel, onClose }) => {
  const [items, setItems] = useState<VesselAvailabilityRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<AvailabilityForm>(() => defaultAvailabilityForm(vessel));
  const { success, error: showError, confirm } = useAlert();

  const load = () => {
    setLoading(true);
    vesselsApi
      .listVesselAvailabilities(vessel.id)
      .then(setItems)
      .catch((e: unknown) => showError(formatApiError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    load();
  }, [vessel.id]);

  const save = async (e: React.FormEvent) => {
    e.preventDefault();
    const from = new Date(form.availableFrom);
    const to = new Date(form.availableTo);
    if (to <= from) {
      showError('Available to must be after available from.');
      return;
    }
    const routeError = validateRoute(form.route, { minStops: 1 });
    if (routeError) {
      showError(routeError);
      return;
    }
    const outside = form.route.findIndex((s) => new Date(s.eta) < from || new Date(s.eta) > to);
    if (outside >= 0) {
      showError(`${stopLabel(outside)}: ETA must fall within the availability window.`);
      return;
    }
    setSaving(true);
    try {
      await vesselsApi.createVesselAvailability({
        vesselId: vessel.id,
        availableFrom: new Date(form.availableFrom).toISOString(),
        availableTo: new Date(form.availableTo).toISOString(),
        routeStops: form.route.map((s) => ({ port: s.port, eta: new Date(s.eta).toISOString() })),
      });
      success('Availability window added. You can now create matches for this vessel.');
      setForm(defaultAvailabilityForm(vessel));
      load();
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  };

  const deactivate = async (item: VesselAvailabilityRecord) => {
    const ok = await confirm({
      title: 'Deactivate window',
      message: 'Deactivate this availability window?',
      confirmText: 'Deactivate',
      variant: 'danger',
    });
    if (!ok) return;
    try {
      await vesselsApi.deleteVesselAvailability(item.id);
      success('Availability window deactivated.');
      load();
    } catch (err) {
      showError(formatApiError(err));
    }
  };

  return (
    <AdminModal
      title={`Availability — ${vessel.name}`}
      onClose={onClose}
      footer={
        <button type="button" className="admin-btn-sm outline" onClick={onClose}>
          Close
        </button>
      }
    >
      <p className="admin-result-text" style={{ marginBottom: '1rem' }}>
        1) Set the window the vessel is available for.<br />
        2) Build the route: start with the <strong>next port</strong>, then <strong>Add port</strong> for each call, each with its ETA.<br />
        3) Click <strong>Add window</strong>. Matching looks for the cargo's loading port, then its discharge port, in that order.
      </p>

      {loading ? (
        <div className="admin-loading">
          <div className="admin-spinner" /> Loading availability…
        </div>
      ) : (
        <>
          {items.length > 0 ? (
            <div className="admin-table-wrap" style={{ marginBottom: '1.25rem' }}>
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>From</th>
                    <th>To</th>
                    <th>Route</th>
                    <th>Status</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {items.map((item) => (
                    <tr key={item.id}>
                      <td>{new Date(item.availableFrom).toLocaleString()}</td>
                      <td>{new Date(item.availableTo).toLocaleString()}</td>
                      <td>{describeRoute(item)}</td>
                      <td>
                        <span className="admin-badge">{item.isActive === false ? 'Inactive' : 'Active'}</span>
                      </td>
                      <td>
                        {item.isActive !== false && (
                          <button
                            type="button"
                            className="admin-btn-sm danger"
                            onClick={() => void deactivate(item)}
                          >
                            Deactivate
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="admin-empty" style={{ marginBottom: '1rem' }}>
              <i className="ri-calendar-line" /> No availability windows yet
            </div>
          )}

          <h4 style={{ margin: '0 0 0.75rem', color: 'var(--admin-navy)' }}>Add availability window</h4>
          <form className="admin-form-grid" onSubmit={(e) => void save(e)}>
            <div className="admin-field">
              <label htmlFor="av-from">Available from</label>
              <input
                id="av-from"
                type="datetime-local"
                className="admin-input"
                required
                value={form.availableFrom}
                onChange={(e) => setForm((f) => ({ ...f, availableFrom: e.target.value }))}
              />
            </div>
            <div className="admin-field">
              <label htmlFor="av-to">Available to</label>
              <input
                id="av-to"
                type="datetime-local"
                className="admin-input"
                required
                value={form.availableTo}
                onChange={(e) => setForm((f) => ({ ...f, availableTo: e.target.value }))}
              />
            </div>
            <div className="admin-field full">
              <label>Route</label>
              <RouteBuilder
                stops={form.route}
                onChange={(route) => { setForm((f) => ({ ...f, route })); }}
                etaType="datetime-local"
                minEta={form.availableFrom}
              />
            </div>
            <div className="admin-field full">
              <button type="submit" className="admin-btn-sm primary" disabled={saving}>
                {saving ? 'Saving…' : 'Add window'}
              </button>
            </div>
          </form>
        </>
      )}
    </AdminModal>
  );
};

export default VesselAvailabilityModal;
