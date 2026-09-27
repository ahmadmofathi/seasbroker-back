import { useState } from 'react';
import type { RequestedQuoteRecord } from '../../api/quote';
import type { PromoteFromQuoteBody } from '../../api/types';
import { CARGO_PRIORITIES, DEFAULT_CARGO_PRIORITY } from '../../constants/domainEnums';
import { cargoTypeSelectOptions } from '../../constants/cargoTypes';
import { portSelectOptions } from '../../utils/portOptions';
import AdminModal from './AdminModal';

type PromoteForm = {
  cargoType: string;
  weight: string;
  dimensions: string;
  departurePort: string;
  departureDate: string;
  arrivalPort: string;
  arrivalDate: string;
  additionalInfo: string;
  priority: string;
};

/** Request dates are stored as the customer typed them (usually YYYY-MM-DD); inputs need YYYY-MM-DD. */
function toDateInput(value?: string): string {
  if (!value) return '';
  if (/^\d{4}-\d{2}-\d{2}/.test(value)) return value.slice(0, 10);
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '' : date.toISOString().slice(0, 10);
}

function fromQuote(q: RequestedQuoteRecord): PromoteForm {
  return {
    cargoType: q.cargoType,
    weight: String(q.weight),
    dimensions: q.dimensions,
    departurePort: q.departurePort,
    departureDate: toDateInput(q.departureTime),
    arrivalPort: q.arrivalPort,
    arrivalDate: toDateInput(q.arrivalTime),
    additionalInfo: q.additionalInfo ?? '',
    priority: String(DEFAULT_CARGO_PRIORITY),
  };
}

const toUtc = (date: string) => new Date(`${date}T00:00:00Z`).toISOString();

interface PromoteCargoModalProps {
  quote: RequestedQuoteRecord;
  saving: boolean;
  onClose: () => void;
  onPromote: (body: PromoteFromQuoteBody) => Promise<void>;
}

/**
 * Lets the admin check and correct a request before it becomes a cargo listing. Only the fields
 * that were changed are sent; the request itself stays exactly as the customer submitted it.
 */
const PromoteCargoModal: React.FC<PromoteCargoModalProps> = ({ quote, saving, onClose, onPromote }) => {
  const original = fromQuote(quote);
  const [form, setForm] = useState<PromoteForm>(original);
  const [error, setError] = useState<string | null>(null);

  const setField = (key: keyof PromoteForm, value: string) => {
    setForm((f) => ({ ...f, [key]: value }));
    setError(null);
  };

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();

    const weight = Number(form.weight);
    if (!(weight > 0)) {
      setError('Weight must be more than 0 MT.');
      return;
    }
    if (!form.departureDate || !form.arrivalDate) {
      setError('Set both the cargo ready date and the estimated arrival date.');
      return;
    }
    if (form.arrivalDate <= form.departureDate) {
      setError('Estimated arrival must be after the cargo ready date.');
      return;
    }

    const changed = (key: keyof PromoteForm) => form[key].trim() !== original[key].trim();
    const body: PromoteFromQuoteBody = {
      requestedQuoteId: quote.id,
      status: 'Open',
      priority: Number(form.priority),
      ...(changed('cargoType') && { cargoType: form.cargoType.trim() }),
      ...(changed('weight') && { weight }),
      ...(changed('dimensions') && { dimensions: form.dimensions.trim() }),
      ...(changed('departurePort') && { departurePort: form.departurePort }),
      ...(changed('departureDate') && { departureTime: toUtc(form.departureDate) }),
      ...(changed('arrivalPort') && { arrivalPort: form.arrivalPort }),
      ...(changed('arrivalDate') && { arrivalTime: toUtc(form.arrivalDate) }),
      ...(changed('additionalInfo') && { additionalInfo: form.additionalInfo.trim() }),
    };

    await onPromote(body);
  };

  return (
    <AdminModal
      title="Review & promote to cargo"
      onClose={onClose}
      footer={
        <>
          <button type="button" className="admin-btn-sm outline" onClick={onClose} disabled={saving}>
            Cancel
          </button>
          <button type="submit" form="promote-form" className="admin-btn-sm primary" disabled={saving}>
            {saving ? 'Promoting…' : 'Promote to Cargo'}
          </button>
        </>
      }
    >
      <p className="admin-result-text" style={{ marginBottom: '1rem' }}>
        Check the details and correct anything before creating the listing. Your changes only apply to
        the new listing - the original request stays as the customer sent it.
      </p>
      <form id="promote-form" className="admin-form-grid" onSubmit={(e) => void submit(e)}>
        <div className="admin-field">
          <label htmlFor="p-type">Cargo Type</label>
          <select id="p-type" className="admin-input" required value={form.cargoType} onChange={(e) => { setField('cargoType', e.target.value); }}>
            {cargoTypeSelectOptions(form.cargoType).map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </div>
        <div className="admin-field">
          <label htmlFor="p-weight">Weight (MT)</label>
          <input id="p-weight" className="admin-input" type="number" min="0" step="any" required value={form.weight} onChange={(e) => { setField('weight', e.target.value); }} />
        </div>
        <div className="admin-field">
          <label htmlFor="p-dep">Departure / Loading Port</label>
          <select id="p-dep" className="admin-input" required value={form.departurePort} onChange={(e) => { setField('departurePort', e.target.value); }}>
            <option value="">Select port…</option>
            {portSelectOptions(form.departurePort).map((p) => (
              <option key={`dep-${p.value}`} value={p.value}>
                {p.label}
              </option>
            ))}
          </select>
        </div>
        <div className="admin-field">
          <label htmlFor="p-arr">Arrival / Discharge Port</label>
          <select id="p-arr" className="admin-input" required value={form.arrivalPort} onChange={(e) => { setField('arrivalPort', e.target.value); }}>
            <option value="">Select port…</option>
            {portSelectOptions(form.arrivalPort).map((p) => (
              <option key={`arr-${p.value}`} value={p.value}>
                {p.label}
              </option>
            ))}
          </select>
        </div>
        <div className="admin-field">
          <label htmlFor="p-ready">Cargo Ready Date</label>
          <input id="p-ready" className="admin-input" type="date" required value={form.departureDate} onChange={(e) => { setField('departureDate', e.target.value); }} />
        </div>
        <div className="admin-field">
          <label htmlFor="p-eta">Estimated Arrival Date</label>
          <input id="p-eta" className="admin-input" type="date" required min={form.departureDate || undefined} value={form.arrivalDate} onChange={(e) => { setField('arrivalDate', e.target.value); }} />
        </div>
        <div className="admin-field">
          <label htmlFor="p-dims">Dimensions</label>
          <input id="p-dims" className="admin-input" value={form.dimensions} onChange={(e) => { setField('dimensions', e.target.value); }} />
        </div>
        <div className="admin-field">
          <label htmlFor="p-priority">Priority</label>
          <select id="p-priority" className="admin-input" value={form.priority} onChange={(e) => { setField('priority', e.target.value); }}>
            {CARGO_PRIORITIES.map((p) => (
              <option key={p} value={String(p)}>
                {p}
              </option>
            ))}
          </select>
        </div>
        <div className="admin-field full">
          <label htmlFor="p-info">Additional Info</label>
          <textarea id="p-info" className="admin-input" rows={5} value={form.additionalInfo} onChange={(e) => { setField('additionalInfo', e.target.value); }} />
        </div>
        {error && (
          <div className="admin-field full" style={{ color: 'var(--admin-danger, #c0392b)' }}>
            {error}
          </div>
        )}
      </form>
    </AdminModal>
  );
};

export default PromoteCargoModal;
