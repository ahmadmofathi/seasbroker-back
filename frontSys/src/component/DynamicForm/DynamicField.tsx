import type { FormField } from '../../api/types';
import type { FieldValue } from './conditionEngine';

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
  Number: 'number',
  Decimal: 'number',
  Date: 'date',
  DateTime: 'datetime-local',
  Time: 'time',
  Email: 'email',
  Phone: 'tel',
};

const DynamicField: React.FC<DynamicFieldProps> = ({ field, value, error, onChange }) => {
  const inputId = `df-${field.key}`;
  const invalidClass = error ? ' is-invalid' : '';

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
          <label className="form-check-label" htmlFor={inputId}>
            {field.placeholder ?? 'Yes'}
          </label>
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

    case 'MultiFile':
      control = (
        <input
          id={inputId}
          type="file"
          multiple
          className={`form-control${invalidClass}`}
          onChange={(e) => onChange(e.target.files ? Array.from(e.target.files) : [])}
        />
      );
      break;

    default:
      control = (
        <input
          id={inputId}
          type={HTML_INPUT_TYPE[field.type] ?? 'text'}
          className={`form-control${invalidClass}`}
          placeholder={field.placeholder ?? undefined}
          value={(value as string) ?? ''}
          onChange={(e) => onChange(e.target.value)}
        />
      );
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
