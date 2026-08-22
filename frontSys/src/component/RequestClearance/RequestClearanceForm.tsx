import { useState, useMemo } from 'react';
import FormSelect from '../Common/FormSelect';
import { getCountryOptions } from '../../utils/portOptions';
import ports from '../../utils/ports.json';
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
      {label}{required && <span className="text-danger"> *</span>}
    </label>
    <div className="custom-file-upload border p-2 rounded d-flex align-items-center justify-content-between bg-light">
      <input type="file" id={id} style={{ display: 'none' }} onChange={(e) => onChange(e.target.files?.[0] ?? null)} />
      <label htmlFor={id} className="btn btn-sm btn-outline-secondary mb-0" style={{ cursor: 'pointer' }}>Choose File</label>
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

// ─── Port state hook (for FormSelect) ─────────────────────────────────────────
function usePortState(initial = '') {
  const [val, setVal] = useState(initial);
  const formData = useMemo(() => ({ port: val }), [val]);
  const setFormData = (arg: { port: string } | ((p: { port: string }) => { port: string })) => {
    if (typeof arg === 'function') setVal(arg({ port: val }).port);
    else setVal(arg.port);
  };
  return { val, formData, setFormData } as const;
}

// ─── Country state hook (same pattern, different key) ────────────────────────
function useCountryState(initial = '') {
  const [val, setVal] = useState(initial);
  const formData = useMemo(() => ({ country: val }), [val]);
  const setFormData = (arg: { country: string } | ((p: { country: string }) => { country: string })) => {
    if (typeof arg === 'function') setVal(arg({ country: val }).country);
    else setVal(arg.country);
  };
  return { val, formData, setFormData } as const;
}

// ─── Main Component ────────────────────────────────────────────────────────────
const RequestClearanceForm: React.FC = () => {
  const navigate = useNavigate();
  const { success, error: showError, warning } = useAlert();
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Options
  const cityOptions = useMemo(() =>
    Object.values(ports).map((p) => ({ text: `${p.name} - ${p.country}`, value: `${p.name} - ${p.country}` }))
  , []);
  const countries = useMemo(() => getCountryOptions(), []);
  const countryOptions = useMemo(() => countries.map((c) => ({ text: c, value: c })), [countries]);

  // ── Clearance Details ──
  const [clearanceType, setClearanceType] = useState('Import');
  const countryOfClearance = useCountryState();
  const portOfClearance = usePortState();
  const countryOfOrigin = useCountryState();
  const countryOfExport = useCountryState();
  const [arrivalDate, setArrivalDate] = useState('');

  // ── Cargo Details ──
  const [cargoType, setCargoType] = useState('Containerized Cargo');
  const [cargoTypeOther, setCargoTypeOther] = useState('');
  const [goodsDescription, setGoodsDescription] = useState('');
  const [hsCode, setHsCode] = useState('');
  const [quantity, setQuantity] = useState('');
  const [grossWeight, setGrossWeight] = useState('');
  const [netWeight, setNetWeight] = useState('');
  const [volume, setVolume] = useState('');
  const [packagingType, setPackagingType] = useState('Cartons');
  const [dangerousGoods, setDangerousGoods] = useState('No');

  // DG fields
  const [unNumber, setUnNumber] = useState('');
  const [imoClass, setImoClass] = useState('');
  const [properShippingName, setProperShippingName] = useState('');
  const [sdsFile, setSdsFile] = useState<File | null>(null);

  // Container-specific
  const [containerType, setContainerType] = useState("20'GP");
  const [containerCount, setContainerCount] = useState('');
  const [blNumber, setBlNumber] = useState('');

  // Breakbulk/Bulk/RoRo-specific
  const [vesselName, setVesselName] = useState('');
  const [voyageNumber, setVoyageNumber] = useState('');
  const [blNumberBulk, setBlNumberBulk] = useState('');

  // ── Commercial / Customs Value ──
  const [invoiceValue, setInvoiceValue] = useState('');
  const [currency, setCurrency] = useState('USD');
  const [incoterm, setIncoterm] = useState('FOB');

  // ── Documents ──
  const [docCommercialInvoice, setDocCommercialInvoice] = useState<File | null>(null);
  const [docPackingList, setDocPackingList] = useState<File | null>(null);
  const [docBillOfLading, setDocBillOfLading] = useState<File | null>(null);
  const [docCertOrigin, setDocCertOrigin] = useState<File | null>(null);
  const [docLicence, setDocLicence] = useState<File | null>(null);
  const [docSdsMsds, setDocSdsMsds] = useState<File | null>(null);
  const [docOther, setDocOther] = useState<File | null>(null);

  // ── Contact ──
  const [companyName, setCompanyName] = useState('');
  const [contactPerson, setContactPerson] = useState('');
  const [email, setEmail] = useState('');
  const [phoneCode, setPhoneCode] = useState('+20');
  const [phone, setPhone] = useState('');
  const [taxNumber, setTaxNumber] = useState('');
  const [remarks, setRemarks] = useState('');

  // ── Helpers ──
  const isContainerized = cargoType === 'Containerized Cargo';
  const isBulkOrRoro = ['General or Breakbulk Cargo', 'Dry Bulk', 'Liquid Bulk', 'RoRo or Vehicles & Equipment'].includes(cargoType);
  const isDG = dangerousGoods === 'Yes';

  // ── Validation ──
  const validate = (): boolean => {
    const e: Record<string, string> = {};
    if (!countryOfClearance.val) e.countryOfClearance = 'Country of clearance is required.';
    if (!portOfClearance.val) e.portOfClearance = 'Port / Customs office is required.';
    if (!countryOfOrigin.val) e.countryOfOrigin = 'Country of origin is required.';
    if (!countryOfExport.val) e.countryOfExport = 'Country of export is required.';
    if (!arrivalDate) e.arrivalDate = 'Expected arrival / departure date is required.';
    if (!goodsDescription.trim()) e.goodsDescription = 'Goods description is required.';
    if (!quantity.trim()) e.quantity = 'Quantity is required.';
    if (!grossWeight.trim()) e.grossWeight = 'Total gross weight is required.';
    if (!packagingType.trim()) e.packagingType = 'Packaging type is required.';
    if (!invoiceValue.trim()) e.invoiceValue = 'Invoice value is required.';
    if (!companyName.trim()) e.companyName = 'Company name is required.';
    if (!contactPerson.trim()) e.contactPerson = 'Contact person is required.';
    if (!/\S+@\S+\.\S+/.test(email)) e.email = 'Valid email is required.';
    if (!phone.trim()) e.phone = 'Phone number is required.';
    if (cargoType === 'Other' && !cargoTypeOther.trim()) e.cargoTypeOther = 'Please describe the cargo type.';
    if (hsCode && !/^\d{6,10}$/.test(hsCode)) e.hsCode = 'HS Code must be 6–10 digits.';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  // ── Serialise ──
  const buildInfo = (): string => {
    const lines: string[] = [];
    const add = (k: string, v: string) => { if (v) lines.push(`${k}: ${v}`); };

    lines.push('=== Clearance Details ===');
    add('Clearance Type', clearanceType);
    add('Country of Clearance', countryOfClearance.val);
    add('Port / Customs Office', portOfClearance.val);
    add('Country of Origin', countryOfOrigin.val);
    add('Country of Export', countryOfExport.val);
    add('Expected Date', arrivalDate);

    lines.push('\n=== Cargo Details ===');
    add('Cargo Type', cargoType === 'Other' ? `Other — ${cargoTypeOther}` : cargoType);
    add('Goods Description', goodsDescription);
    add('HS Code', hsCode || 'N/A');
    add('Quantity', quantity);
    add('Total Gross Weight', grossWeight);
    add('Total Net Weight', netWeight || 'N/A');
    add('Total Volume (m³)', volume || 'N/A');
    add('Packaging Type', packagingType);
    add('Dangerous Goods', dangerousGoods);
    if (isDG) {
      add('UN Number', unNumber || 'N/A');
      add('IMO/IMDG Class', imoClass || 'N/A');
      add('Proper Shipping Name', properShippingName || 'N/A');
      add('SDS/MSDS', sdsFile?.name ?? 'None');
    }
    if (isContainerized) {
      add('Container Type', containerType);
      add('Number of Containers', containerCount || 'N/A');
      add('Bill of Lading Number', blNumber || 'N/A');
    }
    if (isBulkOrRoro) {
      add('Vessel Name', vesselName || 'N/A');
      add('Voyage Number', voyageNumber || 'N/A');
      add('Bill of Lading Number', blNumberBulk || 'N/A');
    }

    lines.push('\n=== Commercial / Customs Value ===');
    add('Invoice Value', `${invoiceValue} ${currency}`);
    add('Incoterm', incoterm);

    lines.push('\n=== Documents Attached ===');
    add('Commercial Invoice', docCommercialInvoice?.name ?? 'None');
    add('Packing List', docPackingList?.name ?? 'None');
    add('Bill of Lading / AWB / CMR', docBillOfLading?.name ?? 'None');
    add('Certificate of Origin', docCertOrigin?.name ?? 'None');
    add('Import/Export Licence', docLicence?.name ?? 'None');
    add('SDS/MSDS', docSdsMsds?.name ?? 'None');
    add('Other Documents', docOther?.name ?? 'None');

    lines.push('\n=== Contact ===');
    add('Company', companyName);
    add('Contact Person', contactPerson);
    add('Email', email);
    add('Phone', `${phoneCode} ${phone}`);
    add('Tax / VAT Number', taxNumber || 'N/A');
    add('Special Instructions', remarks || 'None');

    return lines.join('\n');
  };

  // ── Submit ──
  const handleSubmit: React.FormEventHandler = async (e) => {
    e.preventDefault();
    if (!validate()) { warning('Please correct the validation errors before submitting.'); return; }
    setSubmitting(true);
    try {
      const deptISO = new Date(arrivalDate).toISOString();
      const etaISO  = new Date(new Date(arrivalDate).getTime() + 7 * 864e5).toISOString();

      const resp = await quoteApi.submitQuote({
        cargoType: cargoType === 'Other' ? cargoTypeOther : cargoType,
        weight: Number(grossWeight) || 1,
        departurePort: countryOfOrigin.val,
        departureTime: deptISO,
        arrivalPort: portOfClearance.val,
        arrivalTime: etaISO,
        dimensions: isContainerized ? containerType : packagingType,
        additionalInfo: withServiceTag('Customs Clearance', buildInfo()),
        fname: contactPerson,
        lname: `(${companyName})`,
        email,
        phoneNumber: `${phoneCode} ${phone}`,
      });
      success(resp.message ?? 'Customs Clearance request submitted successfully! Our team will review it shortly.');
      void navigate('/');
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  // ── Reusable field renderer ──
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

  const countrySelect = (
    label: string, errorKey: string, state: ReturnType<typeof useCountryState>,
  ) => (
    <div className="mb-3">
      <FormSelect<{ country: string }>
        label={label}
        placeholder={`Search ${label}…`}
        options={countryOptions}
        formField="country"
        error={errors[errorKey] ?? ''}
        formData={state.formData}
        setFormData={state.setFormData}
      />
      {errors[errorKey] && <div className="text-danger small mt-1">{errors[errorKey]}</div>}
    </div>
  );

  const portSelect = (
    label: string, errorKey: string, state: ReturnType<typeof usePortState>,
  ) => (
    <div className="mb-3">
      <FormSelect<{ port: string }>
        label={label}
        placeholder={`Search ${label}…`}
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
            <form id="customs_clearance_form" onSubmit={handleSubmit} noValidate>
              <div className="row">

                {/* ─── Heading ─── */}
                <div className="col-lg-12">
                  <div className="heading_quote">
                    <h3>Request Customs Clearance</h3>
                  </div>
                </div>

                {/* ══════════════════════════════════════════
                    CLEARANCE DETAILS
                ══════════════════════════════════════════ */}
                <div className="col-lg-12 mb-2">
                  <h5 className="text-secondary"><i className="ri-government-line me-1" />Clearance Details</h5>
                </div>

                <div className="col-lg-6">
                  <div className="form-group mb-3">
                    <label htmlFor="clearanceType" className="form-label">Clearance Type<span className="text-danger"> *</span></label>
                    <select id="clearanceType" className="form-control" value={clearanceType} onChange={(e) => setClearanceType(e.target.value)}>
                      <option>Import</option>
                      <option>Export</option>
                      <option>Transit</option>
                      <option>Temporary Import-Export</option>
                    </select>
                  </div>
                </div>

                <div className="col-lg-6">
                  <div className="form-group mb-3">
                    <label htmlFor="arrivalDate" className="form-label">Expected Arrival / Departure Date<span className="text-danger"> *</span></label>
                    <input
                      id="arrivalDate" type="date"
                      className={`form-control${errors.arrivalDate ? ' is-invalid' : ''}`}
                      value={arrivalDate}
                      onChange={(e) => setArrivalDate(e.target.value)}
                    />
                    {errors.arrivalDate && <div className="invalid-feedback">{errors.arrivalDate}</div>}
                  </div>
                </div>

                <div className="col-lg-6">{countrySelect('Country of Clearance', 'countryOfClearance', countryOfClearance)}</div>
                <div className="col-lg-6">{portSelect('Port / Customs Office', 'portOfClearance', portOfClearance)}</div>
                <div className="col-lg-6">{countrySelect('Country of Origin', 'countryOfOrigin', countryOfOrigin)}</div>
                <div className="col-lg-6">{countrySelect('Country of Export', 'countryOfExport', countryOfExport)}</div>

                {/* ══════════════════════════════════════════
                    CARGO DETAILS
                ══════════════════════════════════════════ */}
                <div className="col-lg-12 border-top pt-3 mt-2 mb-2">
                  <h5 className="text-secondary"><i className="ri-box-3-line me-1" />Cargo Details</h5>
                </div>

                <div className="col-lg-6">
                  <div className="form-group mb-3">
                    <label htmlFor="cargoType" className="form-label">Cargo Type<span className="text-danger"> *</span></label>
                    <select id="cargoType" className="form-control" value={cargoType} onChange={(e) => setCargoType(e.target.value)}>
                      <option>Containerized Cargo</option>
                      <option>General or Breakbulk Cargo</option>
                      <option>Dry Bulk</option>
                      <option>Liquid Bulk</option>
                      <option>Gas Cargo</option>
                      <option>RoRo or Vehicles &amp; Equipment</option>
                      <option>Project &amp; Heavy-Lift Cargo</option>
                      <option>Refrigerated or Perishable Cargo</option>
                      <option>Other</option>
                    </select>
                  </div>
                </div>
                {cargoType === 'Other' && (
                  <div className="col-lg-6">{field('cargoTypeOther', 'Describe Cargo Type', cargoTypeOther, setCargoTypeOther, { required: true, placeholder: 'Describe your cargo type…' })}</div>
                )}

                <div className="col-lg-12">{field('goodsDescription', 'Commodity / Goods Description', goodsDescription, setGoodsDescription, { required: true, placeholder: 'e.g. Steel pipes, Wheat grain, Electronic equipment…' })}</div>

                <div className="col-lg-4">{field('hsCode', 'HS Code (6–10 digits)', hsCode, setHsCode, { placeholder: 'e.g. 720421' })}</div>
                <div className="col-lg-4">{field('quantity', 'Quantity (pcs / packages / units)', quantity, setQuantity, { required: true, type: 'number', placeholder: 'e.g. 200' })}</div>
                <div className="col-lg-4">
                  <div className="form-group mb-3">
                    <label htmlFor="packagingType" className="form-label">Packaging Type<span className="text-danger"> *</span></label>
                    <select id="packagingType" className={`form-control${errors.packagingType ? ' is-invalid' : ''}`} value={packagingType} onChange={(e) => setPackagingType(e.target.value)}>
                      {['Cartons', 'Pallets', 'Crates', 'Bags', 'Bulk', 'Drums', 'IBCs', 'Containers', 'Breakbulk', 'Other'].map((p) => (
                        <option key={p}>{p}</option>
                      ))}
                    </select>
                    {errors.packagingType && <div className="invalid-feedback">{errors.packagingType}</div>}
                  </div>
                </div>

                <div className="col-lg-4">{field('grossWeight', 'Total Gross Weight (kg or MT)', grossWeight, setGrossWeight, { required: true, type: 'number', step: 'any', placeholder: 'e.g. 25000' })}</div>
                <div className="col-lg-4">{field('netWeight', 'Total Net Weight (kg or MT)', netWeight, setNetWeight, { type: 'number', step: 'any', placeholder: 'e.g. 24000' })}</div>
                <div className="col-lg-4">{field('volume', 'Total Volume (m³)', volume, setVolume, { type: 'number', step: 'any', placeholder: 'e.g. 85' })}</div>

                <div className="col-lg-6">
                  <div className="form-group mb-3">
                    <label htmlFor="dangerousGoods" className="form-label">Dangerous Goods?<span className="text-danger"> *</span></label>
                    <select id="dangerousGoods" className="form-control" value={dangerousGoods} onChange={(e) => setDangerousGoods(e.target.value)}>
                      <option>No</option>
                      <option>Yes</option>
                    </select>
                  </div>
                </div>

                {/* ── Dangerous Goods conditional ── */}
                {isDG && (
                  <div className="col-lg-12">
                    <div className="card border-0 bg-light p-3 mb-3">
                      <h6 className="mb-3 text-danger"><i className="ri-alert-line me-1" />Dangerous Goods Details</h6>
                      <div className="row">
                        <div className="col-lg-4">{field('unNumber', 'UN Number', unNumber, setUnNumber, { placeholder: 'e.g. UN 1263' })}</div>
                        <div className="col-lg-4">{field('imoClass', 'IMO / IMDG Class', imoClass, setImoClass, { placeholder: 'e.g. 3' })}</div>
                        <div className="col-lg-4">{field('properShippingName', 'Proper Shipping Name', properShippingName, setProperShippingName, { placeholder: 'e.g. Paint Related Material' })}</div>
                        <div className="col-lg-6">
                          <FileUpload id="sds-file" label="SDS / MSDS" onChange={setSdsFile} selectedFile={sdsFile} />
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                {/* ── Containerized conditional ── */}
                {isContainerized && (
                  <div className="col-lg-12">
                    <div className="card border-0 bg-light p-3 mb-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-ship-line me-1" />Container Details</h6>
                      <div className="row">
                        <div className="col-lg-4">
                          <div className="form-group mb-3">
                            <label htmlFor="containerType" className="form-label">Container Type</label>
                            <select id="containerType" className="form-control" value={containerType} onChange={(e) => setContainerType(e.target.value)}>
                              {["20'GP", "40'GP", "40'HC", 'Reefer', 'OT (Open Top)', 'FR (Flat Rack)', 'Tank Container'].map((t) => (
                                <option key={t}>{t}</option>
                              ))}
                            </select>
                          </div>
                        </div>
                        <div className="col-lg-4">{field('containerCount', 'Number of Containers', containerCount, setContainerCount, { type: 'number', placeholder: 'e.g. 3' })}</div>
                        <div className="col-lg-4">{field('blNumber', 'Bill of Lading Number', blNumber, setBlNumber, { placeholder: 'e.g. MSKU1234567' })}</div>
                      </div>
                    </div>
                  </div>
                )}

                {/* ── Breakbulk / Bulk / RoRo conditional ── */}
                {isBulkOrRoro && (
                  <div className="col-lg-12">
                    <div className="card border-0 bg-light p-3 mb-3">
                      <h6 className="mb-3 text-secondary"><i className="ri-ship-2-line me-1" />Vessel / Shipping Details</h6>
                      <div className="row">
                        <div className="col-lg-4">{field('vesselName', 'Vessel Name', vesselName, setVesselName, { placeholder: 'e.g. Ocean Pioneer' })}</div>
                        <div className="col-lg-4">{field('voyageNumber', 'Voyage Number', voyageNumber, setVoyageNumber, { placeholder: 'e.g. V001E' })}</div>
                        <div className="col-lg-4">{field('blNumberBulk', 'Bill of Lading Number', blNumberBulk, setBlNumberBulk, { placeholder: 'e.g. MAEU123456' })}</div>
                      </div>
                    </div>
                  </div>
                )}

                {/* ══════════════════════════════════════════
                    COMMERCIAL / CUSTOMS VALUE
                ══════════════════════════════════════════ */}
                <div className="col-lg-12 border-top pt-3 mt-2 mb-2">
                  <h5 className="text-secondary"><i className="ri-money-dollar-circle-line me-1" />Commercial / Customs Value</h5>
                </div>

                <div className="col-lg-5">{field('invoiceValue', 'Invoice Value', invoiceValue, setInvoiceValue, { required: true, type: 'number', step: 'any', placeholder: 'e.g. 150000' })}</div>

                <div className="col-lg-3">
                  <div className="form-group mb-3">
                    <label htmlFor="currency" className="form-label">Currency<span className="text-danger"> *</span></label>
                    <select id="currency" className="form-control" value={currency} onChange={(e) => setCurrency(e.target.value)}>
                      {['USD', 'EUR', 'GBP', 'EGP', 'SAR', 'AED', 'CNY', 'JPY'].map((c) => <option key={c}>{c}</option>)}
                    </select>
                  </div>
                </div>

                <div className="col-lg-4">
                  <div className="form-group mb-3">
                    <label htmlFor="incoterm" className="form-label">Incoterm<span className="text-danger"> *</span></label>
                    <select id="incoterm" className="form-control" value={incoterm} onChange={(e) => setIncoterm(e.target.value)}>
                      {['EXW', 'FCA', 'CPT', 'CIP', 'DAP', 'DPU', 'DDP', 'FAS', 'FOB', 'CFR', 'CIF'].map((t) => <option key={t}>{t}</option>)}
                    </select>
                  </div>
                </div>

                {/* ══════════════════════════════════════════
                    DOCUMENTS
                ══════════════════════════════════════════ */}
                <div className="col-lg-12 border-top pt-3 mt-2 mb-2">
                  <h5 className="text-secondary"><i className="ri-folder-open-line me-1" />Transport Documents</h5>
                  <p className="text-muted small mb-3">Upload any available documents. Missing documents can be provided later.</p>
                </div>

                <div className="col-lg-4"><FileUpload id="doc-invoice" label="Commercial Invoice" onChange={setDocCommercialInvoice} selectedFile={docCommercialInvoice} /></div>
                <div className="col-lg-4"><FileUpload id="doc-packing" label="Packing List" onChange={setDocPackingList} selectedFile={docPackingList} /></div>
                <div className="col-lg-4"><FileUpload id="doc-bl" label="Bill of Lading / Air Waybill / CMR" onChange={setDocBillOfLading} selectedFile={docBillOfLading} /></div>
                <div className="col-lg-4"><FileUpload id="doc-origin" label="Certificate of Origin" onChange={setDocCertOrigin} selectedFile={docCertOrigin} /></div>
                <div className="col-lg-4"><FileUpload id="doc-licence" label="Import / Export Licence" onChange={setDocLicence} selectedFile={docLicence} /></div>
                <div className="col-lg-4"><FileUpload id="doc-sds" label="SDS / MSDS" onChange={setDocSdsMsds} selectedFile={docSdsMsds} /></div>
                <div className="col-lg-4"><FileUpload id="doc-other" label="Other Supporting Documents" onChange={setDocOther} selectedFile={docOther} /></div>

                {/* ══════════════════════════════════════════
                    CONTACT
                ══════════════════════════════════════════ */}
                <div className="col-lg-12 border-top pt-3 mt-2">
                  <div className="heading_quote">
                    <h3>Contact Details</h3>
                  </div>
                </div>

                <div className="col-lg-6">{field('companyName', 'Company Name', companyName, setCompanyName, { required: true, placeholder: 'Your company name' })}</div>
                <div className="col-lg-6">{field('contactPerson', 'Contact Person', contactPerson, setContactPerson, { required: true, placeholder: 'Full name' })}</div>
                <div className="col-lg-6">{field('email', 'Email', email, setEmail, { required: true, type: 'email', placeholder: 'customs@company.com' })}</div>

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

                <div className="col-lg-6">{field('taxNumber', 'Tax / VAT Number', taxNumber, setTaxNumber, { placeholder: 'Tax or VAT registration number' })}</div>

                <div className="col-lg-12">
                  <div className="form-group mb-3">
                    <label htmlFor="remarks" className="form-label">Special Instructions / Remarks</label>
                    <textarea
                      id="remarks"
                      className="form-control"
                      rows={4}
                      placeholder="Any special instructions, deadlines, or additional information…"
                      value={remarks}
                      onChange={(e) => setRemarks(e.target.value)}
                    />
                  </div>
                </div>

                {/* ─── Submit ─── */}
                <div className="col-lg-12 mt-2">
                  <div className="quote_submit_button text-end">
                    <button type="submit" className="btn btn-theme" disabled={submitting}>
                      {submitting ? 'Submitting…' : 'Submit Clearance Request'}
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

export default RequestClearanceForm;
