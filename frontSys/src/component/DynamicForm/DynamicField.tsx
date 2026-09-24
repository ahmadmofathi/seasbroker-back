import { useMemo } from 'react';
import type { FormField } from '../../api/types';
import type { FieldValue } from './conditionEngine';
import FormSelect from '../Common/FormSelect';
import PhoneInput from '../Common/PhoneInput';
import RouteBuilder from '../Common/RouteBuilder';
import type { RouteStopValue } from '../../api/types';
import ports from '../../utils/ports.json';

interface DynamicFieldProps {
  field: FormField;
  value: FieldValue;
  error?: string | null;
  onChange: (value: FieldValue) => void;
}

const WIDTH_CLASS: Record<FormField['width'], string> = {
  Full: 'col-12',
  Half: 'col-lg-6',
  Third: 'col-lg-4',
};

const HTML_INPUT_TYPE: Partial<Record<FormField['type'], string>> = {
  Text: 'text',
  Date: 'date',
  DateTime: 'datetime-local',
  Time: 'time',
  Email: 'email',
};

const portOptions = Object.values(ports).map((port) => ({
  text: `${port.name} - ${port.country}`,
  value: `${port.name} - ${port.country}`,
}));

/** Strips letters, '+', and scientific notation while keeping one leading '-' and one '.' -
 * blocks junk keystrokes without breaking legitimate negative (e.g. sub-zero temperatures)
 * or decimal (e.g. draft in metres) values. A leading '-' is kept only when `allowNegative` is
 * set - most numeric fields (weights, counts, capacities) can never legitimately be negative;
 * it's only turned on for fields that can be (e.g. sub-zero temperatures). */
function sanitizeNumeric(raw: string, allowNegative: boolean): string {
  let cleaned = raw.replace(/[^0-9.-]/g, '');
  const negative = allowNegative && cleaned.startsWith('-');
  cleaned = cleaned.replace(/-/g, '');
  const dot = cleaned.indexOf('.');
  if (dot !== -1) {
    cleaned = cleaned.slice(0, dot + 1) + cleaned.slice(dot + 1).replace(/\./g, '');
  }
  return (negative ? '-' : '') + cleaned;
}

function todayMin(type: FormField['type']): string | undefined {
  if (type !== 'Date' && type !== 'DateTime') return undefined;
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const d = String(now.getDate()).padStart(2, '0');
  return type === 'Date' ? `${y}-${m}-${d}` : `${y}-${m}-${d}T00:00`;
}

const DynamicField: React.FC<DynamicFieldProps> = ({ field, value, error, onChange }) => {
  const inputId = `df-${field.key}`;
  const invalidClass = error ? ' is-invalid' : '';
  const portFormData = useMemo(() => ({ port: (value as string) ?? '' }), [value]);

  const label = (
    <label htmlFor={inputId} className="form-label">
      {field.label}
      {field.required && <span className="text-danger"> *</span>}
    </label>
  );

  const help = field.helpText && <div className="form-text">{field.helpText}</div>;
  const feedback = error && <div className="invalid-feedback d-block">{error}</div>;

  let control: React.ReactNode;

  switch (field.type) {
    case 'Textarea':
      control = (
        <textarea
          id={inputId}
          className={`form-control${invalidClass}`}
          placeholder={field.placeholder ?? undefined}
          rows={4}
          value={(value as string) ?? ''}
          onChange={(e) => onChange(e.target.value)}
        />
      );
      break;

    case 'Select':
      control = (
        <select
          id={inputId}
          className={`form-select${invalidClass}`}
          value={(value as string) ?? ''}
          onChange={(e) => onChange(e.target.value)}
        >
          <option value="">{field.placeholder ?? 'Select...'}</option>
          {field.options.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      );
      break;

    case 'Number':
    case 'Decimal':
      control = (
        <input
          id={inputId}
          type="text"
          inputMode="decimal"
          className={`form-control${invalidClass}`}
          placeholder={field.placeholder ?? undefined}
          value={(value as string) ?? ''}
          onChange={(e) => onChange(sanitizeNumeric(e.target.value, field.validation?.allowNegative ?? false))}
        />
      );
      break;

    case 'Phone':
      control = (
        <PhoneInput
          id={inputId}
          defaultValue={(value as string) ?? ''}
          onChange={onChange}
          placeholder={field.placeholder ?? undefined}
          required={field.required}
          invalidClass={invalidClass}
        />
      );
      break;

    case 'Port':
      control = (
        <FormSelect<{ port: string }>
          placeholder={field.placeholder ?? 'Search ports...'}
          options={portOptions}
          formField="port"
          error={error ?? ''}
          formData={portFormData}
          setFormData={(update) => {
            const next = typeof update === 'function' ? update(portFormData) : update;
            onChange(next.port);
          }}
        />
      );
      break;

    case 'Route':
      control = (
        <RouteBuilder
          stops={Array.isArray(value) ? (value as RouteStopValue[]) : []}
          onChange={onChange}
          etaType="date"
          minEta={field.validation?.noPastDates ? todayMin('Date') : undefined}
          maxStops={field.validation?.maxSelections ?? undefined}
          invalid={Boolean(error)}
        />
      );
      break;

    case 'Radio':
      control = (
        <div>
          {field.options.map((o) => (
            <div className="form-check" key={o.value}>
              <input
                className={`form-check-input${invalidClass}`}
                type="radio"
                name={inputId}
                id={`${inputId}-${o.value}`}
                checked={value === o.value}
                onChange={() => onChange(o.value)}
              />
              <label className="form-check-label" htmlFor={`${inputId}-${o.value}`}>
                {o.label}
              </label>
            </div>
          ))}
        </div>
      );
      break;

    case 'MultiSelect': {
      const selected = Array.isArray(value) ? (value as string[]) : [];
      control = (
        <div>
          {field.options.map((o) => {
            const checked = selected.includes(o.value);
            return (
              <div className="form-check" key={o.value}>
                <input
                  className="form-check-input"
                  type="checkbox"
                  id={`${inputId}-${o.value}`}
                  checked={checked}
                  onChange={() =>
                    onChange(checked ? selected.filter((v) => v !== o.value) : [...selected, o.value])
                  }
                />
                <label className="form-check-label" htmlFor={`${inputId}-${o.value}`}>
                  {o.label}
                </label>
              </div>
            );
          })}
        </div>
      );
      break;
    }

    case 'Checkbox':
      control = (
        <div className="form-check">
          <input
            className="form-check-input"
            type="checkbox"
            id={inputId}
            checked={Boolean(value)}
            onChange={(e) => onChange(e.target.checked)}
          />
        </div>
      );
      break;

    case 'Toggle':
      control = (
        <div className="form-check form-switch">
          <input
            className="form-check-input"
            type="checkbox"
            role="switch"
            id={inputId}
            checked={Boolean(value)}
            onChange={(e) => onChange(e.target.checked)}
          />
        </div>
      );
      break;

    case 'File':
      control = (
        <input
          id={inputId}
          type="file"
          className={`form-control${invalidClass}`}
          onChange={(e) => onChange(e.target.files?.[0] ?? null)}
        />
      );
      break;

    case 'MultiFile': {
      // Each pick adds to the existing selection instead of replacing it, so files from
      // different folders can be attached one after another.
      const selected = Array.isArray(value) ? (value as File[]) : [];
      control = (
        <>
          <input
            id={inputId}
            type="file"
            multiple
            className={`form-control${invalidClass}`}
            onChange={(e) => {
              const picked = e.target.files ? Array.from(e.target.files) : [];
              const isDuplicate = (f: File) =>
                selected.some((s) => s.name === f.name && s.size === f.size && s.lastModified === f.lastModified);
              onChange([...selected, ...picked.filter((f) => !isDuplicate(f))]);
              e.target.value = ''; // lets the same file be re-added after removing it
            }}
          />
          {selected.length > 0 && (
            <ul className="list-group mt-2">
              {selected.map((file, index) => (
                <li
                  key={[file.name, file.size, file.lastModified].join('-')}
                  className="list-group-item d-flex justify-content-between align-items-center py-1"
                >
                  <span className="text-truncate me-2">{file.name}</span>
                  <button
                    type="button"
                    className="btn-close"
                    aria-label={`Remove ${file.name}`}
                    onClick={() => { onChange(selected.filter((_, i) => i !== index)); }}
                  />
                </li>
              ))}
            </ul>
          )}
        </>
      );
      break;
    }

    default: {
      const digitsOnly = field.validation?.digitsOnly ?? false;
      const maxLength = field.validation?.maxLength ?? undefined;
      const min = field.validation?.noPastDates ? todayMin(field.type) : undefined;
      const noFutureYear = field.validation?.noFutureYear ?? false;
      const fixedPrefix = field.validation?.fixedPrefix ?? undefined;

      const rawValue = (value as string) ?? '';
      const displayValue = fixedPrefix
        ? rawValue.replace(new RegExp(`^${fixedPrefix}\\s*`), '')
        : rawValue;

      const handleChange = (raw: string) => {
        let next = raw;
        if (digitsOnly) next = next.replace(/\D/g, '');
        if (maxLength != null) next = next.slice(0, maxLength);
        if (noFutureYear && next.length === 4) {
          const currentYear = new Date().getFullYear();
          if (Number(next) > currentYear) next = String(currentYear);
        }
        onChange(fixedPrefix ? (next ? `${fixedPrefix} ${next}` : '') : next);
      };

      const input = (
        <input
          id={inputId}
          type={HTML_INPUT_TYPE[field.type] ?? 'text'}
          className={fixedPrefix ? 'flex-fill border-0 bg-transparent p-0' : `form-control${invalidClass}`}
          style={fixedPrefix ? { outline: 'none' } : undefined}
          placeholder={field.placeholder ?? undefined}
          inputMode={digitsOnly ? 'numeric' : undefined}
          maxLength={digitsOnly ? undefined : maxLength}
          min={min}
          value={displayValue}
          onChange={(e) => handleChange(e.target.value)}
        />
      );

      control = fixedPrefix ? (
        <div className={`form-control d-flex align-items-center gap-2${invalidClass}`}>
          <span className="fw-bold">{fixedPrefix}</span>
          {input}
        </div>
      ) : (
        input
      );
      break;
    }
  }

  const isCheckLike = field.type === 'Checkbox' || field.type === 'Toggle';

  return (
    <div className={WIDTH_CLASS[field.width]}>
      <div className="form-group mb-3">
        {!isCheckLike && label}
        {control}
        {isCheckLike && label}
        {help}
        {feedback}
      </div>
    </div>
  );
};

export default DynamicField;
