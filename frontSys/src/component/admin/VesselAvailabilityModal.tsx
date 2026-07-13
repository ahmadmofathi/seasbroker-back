import { useEffect, useState } from 'react';
import { vesselsApi } from '../../api';
import type { VesselAvailabilityRecord, VesselRecord } from '../../api/types';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import { isKnownPort, portSelectOptions } from '../../utils/portOptions';
import AdminModal from './AdminModal';

type AvailabilityForm = {
  availableFrom: string;
  availableTo: string;
  openPort: string;
  destinationPort: string;
};

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
    openPort: port,
    destinationPort: '',
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
    if (!isKnownPort(form.openPort) || !isKnownPort(form.destinationPort)) {
      showError('Please select valid open and destination ports from the list.');
      return;
    }
    const from = new Date(form.availableFrom);
    const to = new Date(form.availableTo);
    if (to <= from) {
      showError('Available to must be after available from.');
      return;
    }
    setSaving(true);
    try {
      await vesselsApi.createVesselAvailability({
        vesselId: vessel.id,
        availableFrom: new Date(form.availableFrom).toISOString(),
        availableTo: new Date(form.availableTo).toISOString(),
        openPort: form.openPort.trim(),
        destinationPort: form.destinationPort.trim(),
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

  const portOptions = portSelectOptions(form.openPort);

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
        1) Pick real ports from the list (same route as the cargo).<br />
        2) Set dates that <strong>cover</strong> the cargo departure → arrival.<br />
        3) Click <strong>Add window</strong>, then retry Manual Match.
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
                    <th>Open Port</th>
                    <th>Destination</th>
                    <th>Status</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {items.map((item) => (
                    <tr key={item.id}>
                      <td>{new Date(item.availableFrom).toLocaleString()}</td>
                      <td>{new Date(item.availableTo).toLocaleString()}</td>
                      <td>{item.openPort}</td>
                      <td>{item.destinationPort}</td>
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
            <div className="admin-field">
              <label htmlFor="av-open">Open port</label>
              <select
                id="av-open"
                className="admin-input"
                required
                value={form.openPort}
                onChange={(e) => setForm((f) => ({ ...f, openPort: e.target.value }))}
              >
                <option value="">Select port…</option>
                {portOptions.map((p) => (
                  <option key={p.value} value={p.value}>
                    {p.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field">
              <label htmlFor="av-dest">Destination port</label>
              <select
                id="av-dest"
                className="admin-input"
                required
                value={form.destinationPort}
                onChange={(e) => setForm((f) => ({ ...f, destinationPort: e.target.value }))}
              >
                <option value="">Select port…</option>
                {portSelectOptions(form.destinationPort).map((p) => (
                  <option key={`dest-${p.value}`} value={p.value}>
                    {p.label}
                  </option>
                ))}
              </select>
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
