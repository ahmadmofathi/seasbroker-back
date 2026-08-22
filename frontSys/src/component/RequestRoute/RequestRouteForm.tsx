import { useState, useMemo } from 'react';
import FormSelect from '../Common/FormSelect';
import ports from '../../utils/ports.json';
import { getCountryOptions } from '../../utils/portOptions';
import { quoteApi } from '../../api';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import { withServiceTag } from '../../api/types';
import { useNavigate } from 'react-router';

// ─── File Upload ──────────────────────────────────────────────────────────────
const FileUpload: React.FC<{
  id: string;
  label: string;
  required?: boolean;
  onChange: (file: File | null) => void;
  selectedFile: File | null;
}> = ({ id, label, required, onChange, selectedFile }) => (
  <div className="form-group mb-3">
    <label className="form-label">
      {label}
      {required && <span className="text-danger"> *</span>}
    </label>
    <div className="custom-file-upload border p-2 rounded d-flex align-items-center justify-content-between bg-light">
      <input
        type="file"
        id={id}
        style={{ display: 'none' }}
        onChange={(e) => onChange(e.target.files?.[0] ?? null)}
      />
      <label htmlFor={id} className="btn btn-sm btn-outline-secondary mb-0" style={{ cursor: 'pointer' }}>
        Choose File
      </label>
      <span className="text-muted small text-truncate ms-2 flex-grow-1" style={{ maxWidth: '60%' }}>
        {selectedFile ? `${selectedFile.name} (${(selectedFile.size / 1024).toFixed(1)} KB)` : 'No file chosen'}
      </span>
      {selectedFile && (
        <button type="button" className="btn btn-sm text-danger ms-2 p-0 border-0 bg-transparent" onClick={() => onChange(null)}>
          <i className="ri-close-circle-fill fs-5" />
        </button>
      )}
    </div>
  </div>
);

// ─── Checkbox row ─────────────────────────────────────────────────────────────
const CheckRow: React.FC<{ id: string; label: string; checked: boolean; onChange: (v: boolean) => void }> = ({
  id, label, checked, onChange,
}) => (
  <div className="form-check form-check-inline">
    <input type="checkbox" className="form-check-input" id={id} checked={checked} onChange={(e) => onChange(e.target.checked)} />
    <label className="form-check-label" htmlFor={id}>{label}</label>
  </div>
);

// ─── Port select wrapper ───────────────────────────────────────────────────────
// FormSelect requires { formData, setFormData, formField } — we create a thin
// wrapper so each port field can still use a plain string state variable.
function usePortState(initial = '') {
  const [val, setVal] = useState(initial);
  const formData = useMemo(() => ({ port: val }), [val]);
  const setFormData = (arg: { port: string } | ((p: { port: string }) => { port: string })) => {
    if (typeof arg === 'function') setVal(arg({ port: val }).port);
    else setVal(arg.port);
  };
  return { val, formData, setFormData } as const;
}

// ─── Main component ────────────────────────────────────────────────────────────
const RequestRouteForm: React.FC = () => {
  const navigate = useNavigate();
  const { success, error: showError, warning } = useAlert();
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Port options
  const cityOptions = useMemo(() =>
    Object.values(ports).map((p) => ({ text: `${p.name} - ${p.country}`, value: `${p.name} - ${p.country}` }))
  , []);
  const countries = useMemo(() => getCountryOptions(), []);

  // ── Base vessel details ──
  const [vesselName, setVesselName] = useState('');
  const [imoNumber, setImoNumber] = useState('');
  const [vesselType, setVesselType] = useState('Bulk Carrier');
  const [dwt, setDwt] = useState('');
  const [yearBuilt, setYearBuilt] = useState('');
  const [flag, setFlag] = useState('');
  const currentPort = usePortState();
  const [openDateFrom, setOpenDateFrom] = useState('');
  const [openDateTo, setOpenDateTo] = useState('');
  const [draft, setDraft] = useState('');
  const [loa, setLoa] = useState('');
  const [beam, setBeam] = useState('');
  const [gear, setGear] = useState('');
  const [particularsFile, setParticularsFile] = useState<File | null>(null);

  // ── Bulk Carrier ──
  const [bulkGrain, setBulkGrain] = useState('');
  const [bulkHolds, setBulkHolds] = useState('');
  const [bulkGeared, setBulkGeared] = useState(false);
  const [bulkGearless, setBulkGearless] = useState(false);
  const [bulkCrane, setBulkCrane] = useState('');

  // ── Tanker (Oil / Chemical / Product) ──
  const [tankCargo, setTankCargo] = useState('');
  const [tankCount, setTankCount] = useState('');
  const [tankImoClass, setTankImoClass] = useState('');
  const [tankHeating, setTankHeating] = useState(false);
  const [tankCoated, setTankCoated] = useState(false);
  const [tankPumping, setTankPumping] = useState('');

  // ── Container Ship ──
  const [contTeu, setContTeu] = useState('');
  const [contReefer, setContReefer] = useState(false);
  const [contMaxDraft, setContMaxDraft] = useState('');

  // ── RoRo / PCTC ──
  const [roroLane, setRoroLane] = useState('');
  const [roroVehicles, setRoroVehicles] = useState('');
  const [roroRampSwl, setRoroRampSwl] = useState('');
  const [roroRampDims, setRoroRampDims] = useState('');

  // ── Heavy Lift / MPP (General Cargo or Multipurpose) ──
  const [mppDeckArea, setMppDeckArea] = useState('');
  const [mppDeckStrength, setMppDeckStrength] = useState('');
  const [mppCrane, setMppCrane] = useState('');
  const [mppCombined, setMppCombined] = useState('');

  // ── Gas (LNG / LPG) ──
  const [gasCap, setGasCap] = useState('');
  const [gasCargoType, setGasCargoType] = useState('');
  const [gasTankType, setGasTankType] = useState('');
  const [gasMinTemp, setGasMinTemp] = useState('');
  const [gasMaxPres, setGasMaxPres] = useState('');

  // ── Cargo / Employment Preference ──
  const [prefType, setPrefType] = useState('Heavy Lift or Project Cargo Vessel');
  const [prefTypeOther, setPrefTypeOther] = useState('');
  const [acceptedTypes, setAcceptedTypes] = useState<string[]>([]);
  const [minQty, setMinQty] = useState('');
  const [maxQty, setMaxQty] = useState('');
  const [dgAccepted, setDgAccepted] = useState('No');
  const [preferredCommodity, setPreferredCommodity] = useState('');

  // ── Availability ──
  const [availType, setAvailType] = useState('Open Vessel');

  // Open Vessel
  const openPort = usePortState();
  const [openDate, setOpenDate] = useState('');
  const tradingArea = usePortState();
  const destArea = usePortState();

  // Scheduled Route
  const schedDepart = usePortState();
  const [schedDepartDate, setSchedDepartDate] = useState('');
  const schedDest = usePortState();
  const [schedEta, setSchedEta] = useState('');
  const [transitPorts, setTransitPorts] = useState<string[]>([]);
  const transitInput = usePortState();

  // ── Contact ──
  const [companyName, setCompanyName] = useState('');
  const [contactPerson, setContactPerson] = useState('');
  const [position, setPosition] = useState('');
  const [email, setEmail] = useState('');
  const [phoneCode, setPhoneCode] = useState('+20');
  const [phone, setPhone] = useState('');

  // ── Helpers ──
  const isTanker = ['Oil Tanker', 'Chemical Tanker', 'Product Tanker'].includes(vesselType);
  const isMpp = vesselType === 'General Cargo or Multipurpose';
  const isGas = vesselType === 'LNG Carrier' || vesselType === 'LPG Carrier';

  const toggleAccepted = (t: string) =>
    setAcceptedTypes((prev) => prev.includes(t) ? prev.filter((x) => x !== t) : [...prev, t]);

  // Transit port multi-select handler
  const transitSetFormData = (arg: { port: string } | ((p: { port: string }) => { port: string })) => {
    const newVal = typeof arg === 'function' ? arg({ port: transitInput.val }).port : arg.port;
    if (newVal && !transitPorts.includes(newVal)) {
      setTransitPorts((prev) => [...prev, newVal]);
    }
    transitInput.setFormData({ port: '' });
  };

  // ── Validation ──
  const validate = (): boolean => {
    const e: Record<string, string> = {};
    if (!vesselName.trim()) e.vesselName = 'Vessel name is required.';
    if (!/^\d{7}$/.test(imoNumber)) e.imoNumber = 'IMO Number must be exactly 7 digits.';
    if (!dwt || Number(dwt) <= 0) e.dwt = 'DWT is required.';
    if (!/^\d{4}$/.test(yearBuilt)) e.yearBuilt = 'Year built must be a 4-digit year.';
    if (!flag) e.flag = 'Flag country is required.';
    if (!currentPort.val) e.currentPort = 'Current / Open Port is required.';
    if (!openDateFrom) e.openDateFrom = 'Open date from is required.';

    // Vessel-type specifics
    if (vesselType === 'Bulk Carrier') {
      if (!bulkGrain) e.bulkGrain = 'Grain capacity is required.';
      if (!bulkHolds) e.bulkHolds = 'Number of holds/hatches is required.';
      if (!bulkCrane) e.bulkCrane = 'Crane capacity is required.';
    } else if (isTanker) {
      if (!tankCargo) e.tankCargo = 'Cargo capacity is required.';
      if (!tankCount) e.tankCount = 'Number of tanks is required.';
    } else if (vesselType === 'Container Ship') {
      if (!contTeu) e.contTeu = 'TEU capacity is required.';
      if (!contMaxDraft) e.contMaxDraft = 'Max draft is required.';
    } else if (vesselType === 'RoRo or PCTC') {
      if (!roroLane) e.roroLane = 'Lane metres is required.';
      if (!roroVehicles) e.roroVehicles = 'Vehicle capacity is required.';
      if (!roroRampSwl) e.roroRampSwl = 'Ramp SWL is required.';
    } else if (isMpp) {
      if (!mppDeckArea) e.mppDeckArea = 'Deck area is required.';
      if (!mppDeckStrength) e.mppDeckStrength = 'Deck strength is required.';
      if (!mppCrane) e.mppCrane = 'Crane capacity is required.';
    } else if (isGas) {
      if (!gasCap) e.gasCap = 'Cargo capacity is required.';
    }

    if (prefType === 'Other' && !prefTypeOther.trim()) e.prefTypeOther = 'Please specify preference.';

    // Availability
    if (availType === 'Open Vessel') {
      if (!openPort.val) e.openPort = 'Open port is required.';
      if (!openDate) e.openDate = 'Open date is required.';
    } else {
      if (!schedDepart.val) e.schedDepart = 'Departure port is required.';
      if (!schedDepartDate) e.schedDepartDate = 'Departure date is required.';
      if (!schedDest.val) e.schedDest = 'Destination port is required.';
      if (!schedEta) e.schedEta = 'ETA is required.';
    }

    // Contact
    if (!companyName.trim()) e.companyName = 'Company name is required.';
    if (!contactPerson.trim()) e.contactPerson = 'Contact person is required.';
    if (!position.trim()) e.position = 'Position is required.';
    if (!/\S+@\S+\.\S+/.test(email)) e.email = 'Valid email is required.';
    if (!phone.trim()) e.phone = 'Phone number is required.';

    setErrors(e);
    return Object.keys(e).length === 0;
  };

  // ── Serialise to additionalInfo ──
  const buildInfo = (): string => {
    const lines: string[] = [];
    const add = (k: string, v: string) => { if (v) lines.push(`${k}: ${v}`); };

    lines.push('=== Vessel Details ===');
    add('Vessel Name', vesselName);
    add('IMO Number', imoNumber);
    add('Vessel Type', vesselType);
    add('DWT (MT)', dwt);
    add('Year Built', yearBuilt);
    add('Flag', flag);
    add('Current / Open Port', currentPort.val);
    add('Open Date From', openDateFrom);
    add('Open Date To', openDateTo || 'N/A');
    add('Draft (m)', draft || 'N/A');
    add('LOA (m)', loa || 'N/A');
    add('Beam (m)', beam || 'N/A');
    add('Gear', gear || 'N/A');
    add('Vessel Particulars', particularsFile?.name ?? 'None');

    lines.push(`\n=== ${vesselType} Specifications ===`);
    if (vesselType === 'Bulk Carrier') {
      add('Grain Capacity (m³)', bulkGrain);
      add('Holds / Hatches', bulkHolds);
      const feats = [bulkGeared && 'Geared', bulkGearless && 'Gearless'].filter(Boolean).join(', ');
      add('Features', feats || 'None');
      add('Crane Capacity (MT)', bulkCrane);
    } else if (isTanker) {
      add('Cargo Capacity (m³)', tankCargo);
      add('Number of Tanks', tankCount);
      add('IMO Type / Class', tankImoClass || 'N/A');
      const feats = [tankHeating && 'Heating', tankCoated && 'Coated Tanks'].filter(Boolean).join(', ');
      add('Features', feats || 'None');
      add('Pumping Rate (m³/h)', tankPumping || 'N/A');
    } else if (vesselType === 'Container Ship') {
      add('Capacity (TEU)', contTeu);
      add('Reefer Plugs', contReefer ? 'Yes' : 'No');
      add('Max Draft (m)', contMaxDraft);
    } else if (vesselType === 'RoRo or PCTC') {
      add('Lane Metres (m)', roroLane);
      add('Vehicle Capacity', roroVehicles);
      add('Ramp SWL (MT)', roroRampSwl);
      add('Ramp Dimensions (m)', roroRampDims || 'N/A');
    } else if (isMpp) {
      add('Deck Area (m²)', mppDeckArea);
      add('Deck Strength (MT/m²)', mppDeckStrength);
      add('Crane Capacity (MT)', mppCrane);
      add('Combined Lift Capacity (MT)', mppCombined || 'N/A');
    } else if (isGas) {
      add('Cargo Capacity (m³)', gasCap);
      add('Cargo Type', gasCargoType || 'N/A');
      add('Tank Type', gasTankType || 'N/A');
      add('Min Cargo Temp (°C)', gasMinTemp || 'N/A');
      add('Max Working Pressure (bar)', gasMaxPres || 'N/A');
    }

    lines.push('\n=== Cargo / Employment Preference ===');
    add('Preference Type', prefType === 'Other' ? `Other — ${prefTypeOther}` : prefType);
    add('Cargo Types Accepted', acceptedTypes.join(', ') || 'None');
    add('Min Cargo Quantity (MT)', minQty || 'N/A');
    add('Max Cargo Quantity (MT)', maxQty || 'N/A');
    add('Dangerous Goods Accepted', dgAccepted);
    add('Preferred Commodity', preferredCommodity || 'N/A');

    lines.push(`\n=== Availability (${availType}) ===`);
    if (availType === 'Open Vessel') {
      add('Open Port', openPort.val);
      add('Open Date', openDate);
      add('Preferred Trading Area', tradingArea.val || 'N/A');
      add('Preferred Destination Area', destArea.val || 'N/A');
    } else {
      add('Departure Port', schedDepart.val);
      add('Departure Date', schedDepartDate);
      add('Destination Port', schedDest.val);
      add('ETA', schedEta);
      add('Transit Ports', transitPorts.join(', ') || 'None');
    }

    lines.push('\n=== Contact Details ===');
    add('Company', companyName);
    add('Contact Person', contactPerson);
    add('Position', position);
    add('Email', email);
    add('Phone', `${phoneCode} ${phone}`);

    return lines.join('\n');
  };

  // ── Submit ──
  const handleSubmit: React.FormEventHandler = async (e) => {
    e.preventDefault();
    if (!validate()) { warning('Please correct the validation errors before submitting.'); return; }
    setSubmitting(true);
    try {
      const deptPort = availType === 'Open Vessel' ? openPort.val : schedDepart.val;
      const arrPort  = availType === 'Open Vessel' ? (destArea.val || currentPort.val) : schedDest.val;
      const deptDateStr = availType === 'Open Vessel' ? openDate : schedDepartDate;
      const deptISO = new Date(deptDateStr).toISOString();
      const etaISO  = availType === 'Scheduled Route'
        ? new Date(schedEta).toISOString()
        : new Date(new Date(deptDateStr).getTime() + 14 * 864e5).toISOString();

      const resp = await quoteApi.submitQuote({
        cargoType: vesselType,
        weight: Number(dwt) || 0,
        departurePort: deptPort,
        departureTime: deptISO,
        arrivalPort: arrPort,
        arrivalTime: etaISO,
        dimensions: `Draft: ${draft || 'N/A'}m | LOA: ${loa || 'N/A'}m | Beam: ${beam || 'N/A'}m`,
        additionalInfo: withServiceTag('Ship Brokerage', buildInfo()),
        fname: contactPerson,
        lname: `(${companyName})`,
        email,
        phoneNumber: `${phoneCode} ${phone}`,
      });
      success(resp.message ?? 'Ship Brokerage registered successfully! Our team will be in touch.');
      void navigate('/');
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  // ── Shared input helper ──
  const field = (
    id: string, label: string, value: string, setter: (v: string) => void,
    opts: { type?: string; placeholder?: string; required?: boolean; step?: string } = {},
  ) => (
    <div className="form-group mb-3">
      <label htmlFor={id} className="form-label">
        {label}{opts.required && <span className="text-danger"> *</span>}
      </label>
      <input
        id={id} type={opts.type ?? 'text'} step={opts.step}
        className={`form-control${errors[id] ? ' is-invalid' : ''}`}
        placeholder={opts.placeholder ?? ''}
        value={value}
        onChange={(e) => setter(e.target.value)}
      />
      {errors[id] && <div className="invalid-feedback">{errors[id]}</div>}
    </div>
  );

  const portSelect = (
    label: string,
    errorKey: string,
    state: ReturnType<typeof usePortState>,
  ) => (
    <div className="mb-3">
      <FormSelect<{ port: string }>
        label={label}
        placeholder={`Search ${label}...`}
        options={cityOptions}
        formField="port"
        error={errors[errorKey] ?? ''}
        formData={state.formData}
        setFormData={state.setFormData}
      />
      {errors[errorKey] && <div className="text-danger small mt-1">{errors[errorKey]}</div>}
    </div>
  );

  return (
    <section id="request_quote_form_area">
      <div className="container">
        <div className="row">
          <div className="col-lg-12">
            <form id="ship_brokerage_form" onSubmit={handleSubmit} noValidate>
              <div className="row">

                {/* ─── Heading ─── */}
                <div className="col-lg-12">
                  <div className="heading_quote">
                    <h3>Register Ship Brokerage</h3>
                  </div>
                </div>

                {/* ─── Base Vessel Details ─── */}
                <div className="col-lg-6">{field('vesselName', 'Vessel Name', vesselName, setVesselName, { required: true, placeholder: 'e.g. Ocean Pioneer' })}</div>
                <div className="col-lg-6">{field('imoNumber', 'IMO Number', imoNumber, setImoNumber, { required: true, placeholder: '7-digit number' })}</div>

                <div className="col-lg-12">
                  <div className="form-group mb-3">
                    <label htmlFor="vesselType" className="form-label">Vessel Type<span className="text-danger"> *</span></label>
                    <select id="vesselType" className="form-control" value={vesselType} onChange={(e) => setVesselType(e.target.value)}>
                      <option>Bulk Carrier</option>
                      <option>General Cargo or Multipurpose</option>
                      <option>Container Ship</option>
                      <option>RoRo or PCTC</option>
                      <option>Oil Tanker</option>
                      <option>Chemical Tanker</option>
                      <option>Product Tanker</option>
                      <option>LNG Carrier</option>
                      <option>LPG Carrier</option>
                    </select>
                  </div>
                </div>

                {/* ─── Vessel-type specific specs ─── */}
                {vesselType === 'Bulk Carrier' && (
                  <div className="col-lg-12">
                    <div className="card mb-3 border-0 bg-light p-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-anchor-line me-1" />Bulk Carrier Specifications</h6>
                      <div className="row">
                        <div className="col-lg-4">{field('bulkGrain', 'Grain Capacity (m³)', bulkGrain, setBulkGrain, { required: true, type: 'number', placeholder: 'e.g. 52000' })}</div>
                        <div className="col-lg-4">{field('bulkHolds', 'Number of Holds / Hatches', bulkHolds, setBulkHolds, { required: true, type: 'number', placeholder: 'e.g. 7' })}</div>
                        <div className="col-lg-4">{field('bulkCrane', 'Crane Capacity (MT)', bulkCrane, setBulkCrane, { required: true, type: 'number', placeholder: 'e.g. 30' })}</div>
                        <div className="col-lg-12 mb-2">
                          <CheckRow id="bulkGeared" label="Geared" checked={bulkGeared} onChange={setBulkGeared} />
                          <CheckRow id="bulkGearless" label="Gearless" checked={bulkGearless} onChange={setBulkGearless} />
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                {isTanker && (
                  <div className="col-lg-12">
                    <div className="card mb-3 border-0 bg-light p-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-anchor-line me-1" />Tanker Specifications</h6>
                      <div className="row">
                        <div className="col-lg-4">{field('tankCargo', 'Cargo Capacity (m³)', tankCargo, setTankCargo, { required: true, type: 'number', placeholder: 'e.g. 40000' })}</div>
                        <div className="col-lg-4">{field('tankCount', 'Number of Tanks', tankCount, setTankCount, { required: true, type: 'number', placeholder: 'e.g. 12' })}</div>
                        <div className="col-lg-4">{field('tankImoClass', 'IMO Type / Class', tankImoClass, setTankImoClass, { placeholder: 'e.g. IBC Type II' })}</div>
                        <div className="col-lg-4">{field('tankPumping', 'Pumping Rate (m³/h)', tankPumping, setTankPumping, { type: 'number', placeholder: 'e.g. 500' })}</div>
                        <div className="col-lg-12 mb-2">
                          <CheckRow id="tankHeating" label="Heating" checked={tankHeating} onChange={setTankHeating} />
                          <CheckRow id="tankCoated" label="Coated Tanks" checked={tankCoated} onChange={setTankCoated} />
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                {vesselType === 'Container Ship' && (
                  <div className="col-lg-12">
                    <div className="card mb-3 border-0 bg-light p-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-anchor-line me-1" />Container Ship Specifications</h6>
                      <div className="row">
                        <div className="col-lg-4">{field('contTeu', 'Capacity (TEU)', contTeu, setContTeu, { required: true, type: 'number', placeholder: 'e.g. 4200' })}</div>
                        <div className="col-lg-4">{field('contMaxDraft', 'Max Draft (m)', contMaxDraft, setContMaxDraft, { required: true, type: 'number', step: 'any', placeholder: 'e.g. 14.5' })}</div>
                        <div className="col-lg-4 d-flex align-items-center pt-3">
                          <CheckRow id="contReefer" label="Reefer Plugs" checked={contReefer} onChange={setContReefer} />
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                {vesselType === 'RoRo or PCTC' && (
                  <div className="col-lg-12">
                    <div className="card mb-3 border-0 bg-light p-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-anchor-line me-1" />RoRo / PCTC Specifications</h6>
                      <div className="row">
                        <div className="col-lg-3">{field('roroLane', 'Lane Metres (m)', roroLane, setRoroLane, { required: true, type: 'number', step: 'any' })}</div>
                        <div className="col-lg-3">{field('roroVehicles', 'Vehicle Capacity', roroVehicles, setRoroVehicles, { required: true, type: 'number' })}</div>
                        <div className="col-lg-3">{field('roroRampSwl', 'Ramp SWL (MT)', roroRampSwl, setRoroRampSwl, { required: true, type: 'number', step: 'any' })}</div>
                        <div className="col-lg-3">{field('roroRampDims', 'Ramp Dimensions (m)', roroRampDims, setRoroRampDims, { placeholder: 'L × W' })}</div>
                      </div>
                    </div>
                  </div>
                )}

                {isMpp && (
                  <div className="col-lg-12">
                    <div className="card mb-3 border-0 bg-light p-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-anchor-line me-1" />General Cargo / MPP Specifications</h6>
                      <div className="row">
                        <div className="col-lg-3">{field('mppDeckArea', 'Deck Area (m²)', mppDeckArea, setMppDeckArea, { required: true, type: 'number', step: 'any' })}</div>
                        <div className="col-lg-3">{field('mppDeckStrength', 'Deck Strength (MT/m²)', mppDeckStrength, setMppDeckStrength, { required: true, type: 'number', step: 'any' })}</div>
                        <div className="col-lg-3">{field('mppCrane', 'Crane Capacity (MT)', mppCrane, setMppCrane, { required: true, type: 'number', step: 'any' })}</div>
                        <div className="col-lg-3">{field('mppCombined', 'Combined Lift Capacity (MT)', mppCombined, setMppCombined, { type: 'number', step: 'any' })}</div>
                      </div>
                    </div>
                  </div>
                )}

                {isGas && (
                  <div className="col-lg-12">
                    <div className="card mb-3 border-0 bg-light p-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-anchor-line me-1" />{vesselType} Specifications</h6>
                      <div className="row">
                        <div className="col-lg-4">{field('gasCap', 'Cargo Capacity (m³)', gasCap, setGasCap, { required: true, type: 'number' })}</div>
                        <div className="col-lg-4">{field('gasCargoType', 'Cargo Type', gasCargoType, setGasCargoType, { placeholder: 'e.g. Fully Refrigerated' })}</div>
                        <div className="col-lg-4">{field('gasTankType', 'Tank Type', gasTankType, setGasTankType, { placeholder: 'e.g. Membrane' })}</div>
                        <div className="col-lg-4">{field('gasMinTemp', 'Min Cargo Temperature (°C)', gasMinTemp, setGasMinTemp, { type: 'number', step: 'any', placeholder: 'e.g. -163' })}</div>
                        <div className="col-lg-4">{field('gasMaxPres', 'Max Working Pressure (bar)', gasMaxPres, setGasMaxPres, { type: 'number', step: 'any' })}</div>
                      </div>
                    </div>
                  </div>
                )}

                {/* ─── General Particulars (after type specs) ─── */}
                <div className="col-lg-12 border-top pt-3 mt-1 mb-2">
                  <h6 className="text-secondary mb-3"><i className="ri-file-list-3-line me-1" />General Particulars</h6>
                </div>

                <div className="col-lg-4">{field('dwt', 'DWT (MT)', dwt, setDwt, { required: true, type: 'number', placeholder: 'Deadweight tonnage' })}</div>
                <div className="col-lg-4">{field('yearBuilt', 'Year Built', yearBuilt, setYearBuilt, { required: true, placeholder: 'YYYY' })}</div>
                <div className="col-lg-4">
                  <div className="form-group mb-3">
                    <label htmlFor="flag" className="form-label">Flag Country<span className="text-danger"> *</span></label>
                    <select id="flag" className={`form-control${errors.flag ? ' is-invalid' : ''}`} value={flag} onChange={(e) => setFlag(e.target.value)}>
                      <option value="">Select country…</option>
                      {countries.map((c) => <option key={c} value={c}>{c}</option>)}
                    </select>
                    {errors.flag && <div className="invalid-feedback">{errors.flag}</div>}
                  </div>
                </div>

                <div className="col-lg-6">{portSelect('Current / Open Port', 'currentPort', currentPort)}</div>
                <div className="col-lg-3">
                  <div className="form-group mb-3">
                    <label htmlFor="openDateFrom" className="form-label">Open Date From<span className="text-danger"> *</span></label>
                    <input id="openDateFrom" type="date" className={`form-control${errors.openDateFrom ? ' is-invalid' : ''}`} value={openDateFrom} onChange={(e) => setOpenDateFrom(e.target.value)} />
                    {errors.openDateFrom && <div className="invalid-feedback">{errors.openDateFrom}</div>}
                  </div>
                </div>
                <div className="col-lg-3">
                  <div className="form-group mb-3">
                    <label htmlFor="openDateTo" className="form-label">Open Date To</label>
                    <input id="openDateTo" type="date" className="form-control" value={openDateTo} onChange={(e) => setOpenDateTo(e.target.value)} />
                  </div>
                </div>

                <div className="col-lg-4">{field('draft', 'Draft (m)', draft, setDraft, { type: 'number', step: 'any', placeholder: 'e.g. 14.2' })}</div>
                <div className="col-lg-4">{field('loa', 'LOA (m)', loa, setLoa, { type: 'number', step: 'any', placeholder: 'Length overall' })}</div>
                <div className="col-lg-4">{field('beam', 'Beam (m)', beam, setBeam, { type: 'number', step: 'any', placeholder: 'Beam width' })}</div>

                <div className="col-lg-6">
                  <div className="form-group mb-3">
                    <label htmlFor="gear" className="form-label">Gear</label>
                    <select id="gear" className="form-control" value={gear} onChange={(e) => setGear(e.target.value)}>
                      <option value="">Select…</option>
                      <option>Geared</option>
                      <option>Gearless</option>
                    </select>
                  </div>
                </div>
                <div className="col-lg-6">
                  <FileUpload id="vessel-particulars" label="Vessel Particulars" onChange={setParticularsFile} selectedFile={particularsFile} />
                </div>

                {/* ─── Cargo / Employment Preference ─── */}
                <div className="col-lg-12 border-top pt-3 mt-2">
                  <h5 className="mb-3 text-secondary"><i className="ri-list-check-3 me-1" />Cargo / Employment Preference</h5>
                </div>

                <div className="col-lg-6">
                  <div className="form-group mb-3">
                    <label htmlFor="prefType" className="form-label">Preference Type</label>
                    <select id="prefType" className="form-control" value={prefType} onChange={(e) => setPrefType(e.target.value)}>
                      <option>Heavy Lift or Project Cargo Vessel</option>
                      <option>Reefer Vessel</option>
                      <option>Other</option>
                    </select>
                  </div>
                </div>
                {prefType === 'Other' && (
                  <div className="col-lg-6">{field('prefTypeOther', 'Specify Preference', prefTypeOther, setPrefTypeOther, { required: true, placeholder: 'Describe preference…' })}</div>
                )}

                <div className="col-lg-12">
                  <div className="form-group mb-3">
                    <label className="form-label d-block">Cargo Types Accepted</label>
                    <div className="d-flex flex-wrap gap-3 pt-1">
                      {['Dry Bulk', 'General & Breakbulk', 'Project & Heavy Lift', 'Containerized', 'RoRo', 'Liquid Bulk', 'Gas Cargo', 'Refrigerated & Perishable'].map((t) => (
                        <CheckRow key={t} id={`acc-${t}`} label={t} checked={acceptedTypes.includes(t)} onChange={() => toggleAccepted(t)} />
                      ))}
                    </div>
                  </div>
                </div>

                <div className="col-lg-4">{field('minQty', 'Minimum Cargo Quantity (MT)', minQty, setMinQty, { type: 'number', placeholder: 'Min MT' })}</div>
                <div className="col-lg-4">{field('maxQty', 'Maximum Cargo Quantity (MT)', maxQty, setMaxQty, { type: 'number', placeholder: 'Max MT' })}</div>
                <div className="col-lg-4">
                  <div className="form-group mb-3">
                    <label htmlFor="dgAccepted" className="form-label">Dangerous Goods Accepted?</label>
                    <select id="dgAccepted" className="form-control" value={dgAccepted} onChange={(e) => setDgAccepted(e.target.value)}>
                      <option>No</option>
                      <option>Yes</option>
                    </select>
                  </div>
                </div>
                <div className="col-lg-6">{field('preferredCommodity', 'Preferred Cargo / Commodity', preferredCommodity, setPreferredCommodity, { placeholder: 'e.g. Grain, Steel Coils' })}</div>

                {/* ─── Availability Details ─── */}
                <div className="col-lg-12 border-top pt-3 mt-2">
                  <h5 className="mb-3 text-secondary"><i className="ri-map-pin-time-line me-1" />Availability Details</h5>
                </div>

                <div className="col-lg-4">
                  <div className="form-group mb-3">
                    <label htmlFor="availType" className="form-label">Availability Type<span className="text-danger"> *</span></label>
                    <select id="availType" className="form-control" value={availType} onChange={(e) => setAvailType(e.target.value)}>
                      <option>Open Vessel</option>
                      <option>Scheduled Route</option>
                    </select>
                  </div>
                </div>

                {availType === 'Open Vessel' && (<>
                  <div className="col-lg-4">{portSelect('Open Port', 'openPort', openPort)}</div>
                  <div className="col-lg-4">
                    <div className="form-group mb-3">
                      <label htmlFor="openDate" className="form-label">Open Date<span className="text-danger"> *</span></label>
                      <input id="openDate" type="date" className={`form-control${errors.openDate ? ' is-invalid' : ''}`} value={openDate} onChange={(e) => setOpenDate(e.target.value)} />
                      {errors.openDate && <div className="invalid-feedback">{errors.openDate}</div>}
                    </div>
                  </div>
                  <div className="col-lg-6">{portSelect('Preferred Trading Area', 'tradingArea', tradingArea)}</div>
                  <div className="col-lg-6">{portSelect('Preferred Destination Area', 'destArea', destArea)}</div>
                </>)}

                {availType === 'Scheduled Route' && (<>
                  <div className="col-lg-6">{portSelect('Departure Port', 'schedDepart', schedDepart)}</div>
                  <div className="col-lg-6">
                    <div className="form-group mb-3">
                      <label htmlFor="schedDepartDate" className="form-label">Departure Date<span className="text-danger"> *</span></label>
                      <input id="schedDepartDate" type="date" className={`form-control${errors.schedDepartDate ? ' is-invalid' : ''}`} value={schedDepartDate} onChange={(e) => setSchedDepartDate(e.target.value)} />
                      {errors.schedDepartDate && <div className="invalid-feedback">{errors.schedDepartDate}</div>}
                    </div>
                  </div>
                  <div className="col-lg-6">{portSelect('Destination Port', 'schedDest', schedDest)}</div>
                  <div className="col-lg-6">
                    <div className="form-group mb-3">
                      <label htmlFor="schedEta" className="form-label">ETA (date &amp; time)<span className="text-danger"> *</span></label>
                      <input id="schedEta" type="datetime-local" className={`form-control${errors.schedEta ? ' is-invalid' : ''}`} value={schedEta} onChange={(e) => setSchedEta(e.target.value)} />
                      {errors.schedEta && <div className="invalid-feedback">{errors.schedEta}</div>}
                    </div>
                  </div>

                  {/* Transit Ports multi-select */}
                  <div className="col-lg-12">
                    <FormSelect<{ port: string }>
                      label="Add Transit Port"
                      placeholder="Search and add transit ports…"
                      options={cityOptions}
                      formField="port"
                      error=""
                      formData={transitInput.formData}
                      setFormData={transitSetFormData}
                    />
                    {transitPorts.length > 0 && (
                      <div className="mb-3 d-flex flex-wrap gap-2">
                        {transitPorts.map((p) => (
                          <span
                            key={p}
                            className="badge bg-secondary p-2 d-flex align-items-center"
                            style={{ cursor: 'pointer' }}
                            onClick={() => setTransitPorts((prev) => prev.filter((x) => x !== p))}
                          >
                            {p} <i className="ri-close-line ms-1" />
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                </>)}

                {/* ─── Contact Details ─── */}
                <div className="col-lg-12 border-top pt-3 mt-2">
                  <div className="heading_quote">
                    <h3>Contact Details</h3>
                  </div>
                </div>

                <div className="col-lg-6">{field('companyName', 'Company Name', companyName, setCompanyName, { required: true, placeholder: 'Company name' })}</div>
                <div className="col-lg-6">{field('contactPerson', 'Contact Person', contactPerson, setContactPerson, { required: true, placeholder: 'Full name' })}</div>
                <div className="col-lg-6">{field('position', 'Position / Job Title', position, setPosition, { required: true, placeholder: 'e.g. Chartering Manager' })}</div>
                <div className="col-lg-6">{field('email', 'Email', email, setEmail, { required: true, type: 'email', placeholder: 'chartering@company.com' })}</div>

                <div className="col-lg-6">
                  <div className="form-group mb-3">
                    <label htmlFor="phone" className="form-label">Phone Number<span className="text-danger"> *</span></label>
                    <div className="input-group">
                      <select className="form-control flex-shrink-0" style={{ maxWidth: 130 }} value={phoneCode} onChange={(e) => setPhoneCode(e.target.value)}>
                        {['+20','+966','+971','+965','+974','+968','+973','+962','+961','+1','+44'].map((c) => (
                          <option key={c} value={c}>{c}</option>
                        ))}
                      </select>
                      <input id="phone" type="tel" className={`form-control${errors.phone ? ' is-invalid' : ''}`} placeholder="Phone number" value={phone} onChange={(e) => setPhone(e.target.value)} />
                    </div>
                    {errors.phone && <div className="text-danger small mt-1">{errors.phone}</div>}
                  </div>
                </div>

                {/* ─── Submit ─── */}
                <div className="col-lg-12 mt-4">
                  <div className="quote_submit_button text-end">
                    <button type="submit" className="btn btn-theme" disabled={submitting}>
                      {submitting ? 'Registering…' : 'Register Ship Brokerage'}
                    </button>
                  </div>
                </div>

              </div>
            </form>
          </div>
        </div>
      </div>
    </section>
  );
};

export default RequestRouteForm;
