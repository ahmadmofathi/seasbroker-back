import { useEffect, useState } from 'react';
import { vesselsApi } from '../../api';
import AdminModal from '../../component/admin/AdminModal';
import VesselAvailabilityModal from '../../component/admin/VesselAvailabilityModal';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import type { VesselRecord, VesselStatus } from '../../api/types';
import {
  DEFAULT_VESSEL_TYPE,
  vesselTypeSelectOptions,
} from '../../constants/vesselTypes';
import { VESSEL_STATUSES } from '../../constants/domainEnums';
import { countrySelectOptions, portSelectOptions } from '../../utils/portOptions';

type VesselForm = {
  name: string;
  imoNumber: string;
  vesselType: string;
  dwt: string;
  lengthOverall: string;
  beam: string;
  draft: string;
  currentPort: string;
  flagCountry: string;
  status: VesselStatus;
  notes: string;
};

const emptyForm = (): VesselForm => ({
  name: '',
  imoNumber: '',
  vesselType: DEFAULT_VESSEL_TYPE,
  dwt: '',
  lengthOverall: '',
  beam: '',
  draft: '',
  currentPort: '',
  flagCountry: '',
  status: 'Active',
  notes: '',
});

function fromVessel(v: VesselRecord): VesselForm {
  return {
    name: v.name ?? '',
    imoNumber: v.imoNumber ?? '',
    vesselType: v.vesselType?.trim() || DEFAULT_VESSEL_TYPE,
    dwt: String(v.dwt ?? ''),
    lengthOverall: String(v.lengthOverall ?? ''),
    beam: String(v.beam ?? ''),
    draft: String(v.draft ?? ''),
    currentPort: v.currentPort ?? '',
    flagCountry: v.flagCountry ?? '',
    status: v.status ?? 'Active',
    notes: v.notes ?? '',
  };
}

function toPayload(form: VesselForm): Partial<VesselRecord> {
  return {
    name: form.name.trim(),
    imoNumber: form.imoNumber.trim(),
    vesselType: form.vesselType.trim(),
    dwt: Number(form.dwt) || 0,
    lengthOverall: Number(form.lengthOverall) || 0,
    beam: Number(form.beam) || 0,
    draft: Number(form.draft) || 0,
    currentPort: form.currentPort.trim(),
    flagCountry: form.flagCountry.trim(),
    status: form.status,
    notes: form.notes.trim() || undefined,
  };
}

const AdminVessels: React.FC = () => {
  const [vessels, setVessels] = useState<VesselRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<VesselForm>(emptyForm);
  const [editing, setEditing] = useState<VesselRecord | null>(null);
  const [open, setOpen] = useState(false);
  const [availabilityVessel, setAvailabilityVessel] = useState<VesselRecord | null>(null);
  const { error: showError, confirm, success } = useAlert();

  const load = () => {
    setLoading(true);
    vesselsApi
      .listVessels()
      .then(setVessels)
      .catch((e: unknown) => showError(formatApiError(e)))
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

  const openEdit = (v: VesselRecord) => {
    setEditing(v);
    setForm(fromVessel(v));
    setOpen(true);
  };

  const closeModal = () => {
    if (saving) return;
    setOpen(false);
    setEditing(null);
  };

  const setField = <K extends keyof VesselForm>(key: K, value: VesselForm[K]) => {
    setForm((prev) => ({ ...prev, [key]: value }));
  };

  const save = async (e: React.FormEvent) => {
    e.preventDefault();
    const imo = form.imoNumber.trim();
    if (imo && !/^\d{7}$/.test(imo)) {
      showError('IMO Number must be exactly 7 digits.');
      return;
    }
    setSaving(true);
    try {
      const body = toPayload(form);
      if (editing) {
        await vesselsApi.updateVessel(editing.id, body);
      } else {
        const created = await vesselsApi.createVessel(body);
        success(`Vessel "${created.name}" created. Add an availability window to enable matching.`);
        setAvailabilityVessel(created);
      }
      setOpen(false);
      setEditing(null);
      load();
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  };

  const remove = async (v: VesselRecord) => {
    const ok = await confirm({
      title: 'Delete vessel',
      message: `Delete vessel "${v.name}"?`,
      confirmText: 'Delete',
      variant: 'danger',
    });
    if (!ok) return;
    try {
      await vesselsApi.deleteVessel(v.id);
      load();
    } catch (err) {
      showError(formatApiError(err));
    }
  };

  return (
    <>
      <div className="admin-action-bar">
        <button type="button" className="admin-btn-sm primary" onClick={openCreate}>
          <i className="ri-add-line" /> Add Vessel
        </button>
      </div>

      {loading ? (
        <div className="admin-loading">
          <div className="admin-spinner" /> Loading vessels…
        </div>
      ) : (
        <div className="admin-panel">
          <div className="admin-panel-header">
            <h2>Fleet ({vessels.length})</h2>
          </div>
          <div className="admin-panel-body no-pad">
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Vessel Name</th>
                    <th>IMO</th>
                    <th>Type</th>
                    <th>DWT</th>
                    <th>Current Port</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {vessels.map((v) => (
                    <tr key={v.id}>
                      <td style={{ fontWeight: 500, color: 'var(--admin-navy)' }}>{v.name}</td>
                      <td>{v.imoNumber}</td>
                      <td>{v.vesselType}</td>
                      <td>{v.dwt?.toLocaleString()}</td>
                      <td>{v.currentPort}</td>
                      <td>
                        <span className="admin-badge">{v.status}</span>
                      </td>
                      <td>
                        <div className="admin-actions-cell">
                          <button
                            type="button"
                            className="admin-btn-sm primary"
                            onClick={() => setAvailabilityVessel(v)}
                          >
                            Availability
                          </button>
                          <button
                            type="button"
                            className="admin-btn-sm outline"
                            onClick={() => openEdit(v)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className="admin-btn-sm danger"
                            onClick={() => void remove(v)}
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {vessels.length === 0 && (
                <div className="admin-empty">
                  <i className="ri-anchor-line" /> No vessels registered
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {open && (
        <AdminModal
          title={editing ? 'Edit Vessel' : 'Add Vessel'}
          onClose={closeModal}
          footer={
            <>
              <button type="button" className="admin-btn-sm outline" onClick={closeModal} disabled={saving}>
                Cancel
              </button>
              <button type="submit" form="vessel-form" className="admin-btn-sm primary" disabled={saving}>
                {saving ? 'Saving…' : editing ? 'Save Changes' : 'Create Vessel'}
              </button>
            </>
          }
        >
          <form id="vessel-form" className="admin-form-grid" onSubmit={(e) => void save(e)}>
            <div className="admin-field">
              <label htmlFor="v-name">Name</label>
              <input id="v-name" className="admin-input" required value={form.name} onChange={(e) => setField('name', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="v-imo">IMO Number</label>
              <input id="v-imo" className="admin-input" required maxLength={7} inputMode="numeric" value={form.imoNumber} onChange={(e) => setField('imoNumber', e.target.value.replace(/\D/g, '').slice(0, 7))} />
            </div>
            <div className="admin-field">
              <label htmlFor="v-type">Vessel Type</label>
              <select
                id="v-type"
                className="admin-input"
                required
                value={form.vesselType}
                onChange={(e) => setField('vesselType', e.target.value)}
              >
                {vesselTypeSelectOptions(form.vesselType).map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field">
              <label htmlFor="v-dwt">DWT</label>
              <input id="v-dwt" className="admin-input" type="number" min="0" required value={form.dwt} onChange={(e) => setField('dwt', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="v-loa">Length Overall</label>
              <input id="v-loa" className="admin-input" type="number" min="0" step="0.01" required value={form.lengthOverall} onChange={(e) => setField('lengthOverall', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="v-beam">Beam</label>
              <input id="v-beam" className="admin-input" type="number" min="0" step="0.01" required value={form.beam} onChange={(e) => setField('beam', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="v-draft">Draft</label>
              <input id="v-draft" className="admin-input" type="number" min="0" step="0.01" required value={form.draft} onChange={(e) => setField('draft', e.target.value)} />
            </div>
            <div className="admin-field">
              <label htmlFor="v-port">Current Port</label>
              <select
                id="v-port"
                className="admin-input"
                required
                value={form.currentPort}
                onChange={(e) => setField('currentPort', e.target.value)}
              >
                <option value="">Select port…</option>
                {portSelectOptions(form.currentPort).map((p) => (
                  <option key={p.value} value={p.value}>
                    {p.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field">
              <label htmlFor="v-flag">Flag Country</label>
              <select
                id="v-flag"
                className="admin-input"
                required
                value={form.flagCountry}
                onChange={(e) => setField('flagCountry', e.target.value)}
              >
                <option value="">Select country…</option>
                {countrySelectOptions(form.flagCountry).map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field">
              <label htmlFor="v-status">Status</label>
              <select id="v-status" className="admin-input" value={form.status} onChange={(e) => setField('status', e.target.value as VesselStatus)}>
                {VESSEL_STATUSES.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
            </div>
            <div className="admin-field full">
              <label htmlFor="v-notes">Notes</label>
              <input id="v-notes" className="admin-input" value={form.notes} onChange={(e) => setField('notes', e.target.value)} />
            </div>
          </form>
        </AdminModal>
      )}

      {availabilityVessel && (
        <VesselAvailabilityModal
          vessel={availabilityVessel}
          onClose={() => setAvailabilityVessel(null)}
        />
      )}
    </>
  );
};

export default AdminVessels;
