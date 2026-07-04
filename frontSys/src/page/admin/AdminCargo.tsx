import { useEffect, useState } from 'react';
import { cargoApi } from '../../api';
import AdminModal from '../../component/admin/AdminModal';
import { formatApiError } from '../../utils/formatApiError';
import type { CargoListingRecord, CargoStatus } from '../../api/types';

type CargoForm = {
  customer: string;
  cargoType: string;
  weight: string;
  dimensions: string;
  departurePort: string;
  departureTime: string;
  arrivalPort: string;
  arrivalTime: string;
  referenceNumber: string;
  status: CargoStatus;
  priority: string;
  additionalInfo: string;
};

const emptyForm = (): CargoForm => ({
  customer: '',
  cargoType: '',
  weight: '',
  dimensions: '',
  departurePort: '',
  departureTime: '',
  arrivalPort: '',
  arrivalTime: '',
  referenceNumber: '',
  status: 'Open',
  priority: '0',
  additionalInfo: '',
});

function toLocalInput(value?: string): string {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value.slice(0, 16);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function fromCargo(c: CargoListingRecord): CargoForm {
  return {
    customer: c.customer ?? '',
    cargoType: c.cargoType ?? '',
    weight: String(c.weight ?? ''),
    dimensions: c.dimensions ?? '',
    departurePort: c.departurePort ?? '',
    departureTime: toLocalInput(c.departureTime),
    arrivalPort: c.arrivalPort ?? '',
    arrivalTime: toLocalInput(c.arrivalTime),
    referenceNumber: c.referenceNumber ?? '',
    status: c.status ?? 'Open',
    priority: String(c.priority ?? 0),
    additionalInfo: c.additionalInfo ?? '',
  };
}

function toPayload(form: CargoForm): Partial<CargoListingRecord> {
  return {
    customer: form.customer.trim(),
    cargoType: form.cargoType.trim(),
    weight: Number(form.weight) || 0,
    dimensions: form.dimensions.trim(),
    departurePort: form.departurePort.trim(),
    departureTime: form.departureTime ? new Date(form.departureTime).toISOString() : '',
    arrivalPort: form.arrivalPort.trim(),
    arrivalTime: form.arrivalTime ? new Date(form.arrivalTime).toISOString() : '',
    referenceNumber: form.referenceNumber.trim() || undefined,
    status: form.status,
    priority: Number(form.priority) || 0,
    additionalInfo: form.additionalInfo.trim() || undefined,
  };
}

const AdminCargo: React.FC = () => {
  const [listings, setListings] = useState<CargoListingRecord[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<CargoForm>(emptyForm);
  const [editing, setEditing] = useState<CargoListingRecord | null>(null);
  const [open, setOpen] = useState(false);

  const load = () => {
    setLoading(true);
    setError('');
    cargoApi
      .listCargoListings()
      .then(setListings)
      .catch((e: unknown) => setError(formatApiError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    load();
  }, []);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm());
    setOpen(true);
  };

  const openEdit = (c: CargoListingRecord) => {
    setEditing(c);
    setForm(fromCargo(c));
    setOpen(true);
  };

  const closeModal = () => {
    if (saving) return;
    setOpen(false);
    setEditing(null);
  };

  const setField = <K extends keyof CargoForm>(key: K, value: CargoForm[K]) => {
    setForm((prev) => ({ ...prev, [key]: value }));
  };

  const save = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      const body = toPayload(form);
      if (editing) {
        await cargoApi.updateCargoListing(editing.id, body);
      } else {
        await cargoApi.createCargoListing(body);
      }
      setOpen(false);
      setEditing(null);
      load();
    } catch (err) {
      setError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  };

  const closeListing = async (id: string) => {
    try {
      await cargoApi.closeCargo(id);
      load();
    } catch (e) {
      setError(formatApiError(e));
    }
  };

  const cancelListing = async (id: string) => {
    try {
      await cargoApi.cancelCargo(id);
      load();
    } catch (e) {
      setError(formatApiError(e));
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
        <button type="button" className="admin-btn-sm primary" onClick={openCreate}>
          <i className="ri-add-line" /> Add Cargo Listing
        </button>
      </div>

      {loading ? (
        <div className="admin-loading">
          <div className="admin-spinner" /> Loading cargo listings…
        </div>
      ) : (
        <div className="admin-panel">
          <div className="admin-panel-header">
            <h2>Cargo Listings ({listings.length})</h2>
          </div>
          <div className="admin-panel-body no-pad">
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Reference</th>
                    <th>Type</th>
                    <th>Route</th>
                    <th>Weight</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {listings.map((c) => (
                    <tr key={c.id}>
                      <td style={{ fontWeight: 500, color: 'var(--admin-navy)' }}>
                        {c.referenceNumber ?? c.id.slice(0, 8)}
                      </td>
                      <td>{c.cargoType}</td>
                      <td>
                        {c.departurePort} → {c.arrivalPort}
                      </td>
                      <td>{c.weight} kg</td>
                      <td>
                        <span className="admin-badge">{c.status}</span>
                      </td>
                      <td>
                        <div className="admin-actions-cell">
                          <button
                            type="button"
                            className="admin-btn-sm outline"
                            onClick={() => openEdit(c)}
                          >
                            Edit
                          </button>
                          {c.status === 'Open' && (
                            <>
                              <button
                                type="button"
                                className="admin-btn-sm outline"
                                onClick={() => void closeListing(c.id)}
                              >
                                Close
                              </button>
                              <button
                                type="button"
                                className="admin-btn-sm danger"
                                onClick={() => void cancelListing(c.id)}
                              >
                                Cancel
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {listings.length === 0 && (
                <div className="admin-empty">
                  <i className="ri-ship-line" /> No cargo listings
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {open && (
        <AdminModal
          title={editing ? 'Edit Cargo Listing' : 'Add Cargo Listing'}
          onClose={closeModal}
          footer={
            <>
              <button type="button" className="admin-btn-sm outline" onClick={closeModal} disabled={saving}>
                Cancel
              </button>
              <button type="submit" form="cargo-form" className="admin-btn-sm primary" disabled={saving}>
                {saving ? 'Saving…' : editing ? 'Save Changes' : 'Create Listing'}
              </button>
            </>
          }
        >
          <form id="cargo-form" className="admin-form-grid" onSubmit={(e) => void save(e)}>
            <div className="admin-field">
              <label htmlFor="c-customer">Customer ID</label>
              <input id="c-customer" className="admin-input" required value={form.customer} onChange={(e) => setField('customer', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-type">Cargo Type</label>
              <input id="c-type" className="admin-input" required value={form.cargoType} onChange={(e) => setField('cargoType', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-weight">Weight (kg)</label>
              <input id="c-weight" className="admin-input" type="number" min="0" required value={form.weight} onChange={(e) => setField('weight', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-dims">Dimensions</label>
              <input id="c-dims" className="admin-input" required value={form.dimensions} onChange={(e) => setField('dimensions', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-dep">Departure Port</label>
              <input id="c-dep" className="admin-input" required value={form.departurePort} onChange={(e) => setField('departurePort', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-arr">Arrival Port</label>
              <input id="c-arr" className="admin-input" required value={form.arrivalPort} onChange={(e) => setField('arrivalPort', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-dep-time">Departure Time</label>
              <input id="c-dep-time" className="admin-input" type="datetime-local" required value={form.departureTime} onChange={(e) => setField('departureTime', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-arr-time">Arrival Time</label>
              <input id="c-arr-time" className="admin-input" type="datetime-local" required value={form.arrivalTime} onChange={(e) => setField('arrivalTime', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-ref">Reference Number</label>
              <input id="c-ref" className="admin-input" value={form.referenceNumber} onChange={(e) => setField('referenceNumber', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-priority">Priority</label>
              <input id="c-priority" className="admin-input" type="number" value={form.priority} onChange={(e) => setField('priority', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="c-status">Status</label>
              <select id="c-status" className="admin-input" value={form.status} onChange={(e) => setField('status', e.target.value as CargoStatus)}>
                <option value="Draft">Draft</option>
                <option value="Open">Open</option>
                <option value="Matched">Matched</option>
                <option value="Closed">Closed</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
            <div className="admin-field full">
              <label htmlFor="c-info">Additional Info</label>
              <input id="c-info" className="admin-input" value={form.additionalInfo} onChange={(e) => setField('additionalInfo', e.target.value)} />
            </div>
          </form>
        </AdminModal>
      )}
    </>
  );
};

export default AdminCargo;
