import { useRef } from 'react';
import { COUNTRY_CODES, splitPhone } from '../../utils/countryCodes';

interface PhoneInputProps {
  id?: string;
  /** Set when embedding in an uncontrolled form read via FormData on submit (e.g. Contact Us). */
  name?: string;
  /** Combined value, e.g. "+20 1012345678". Only read once, on mount. */
  defaultValue?: string;
  placeholder?: string;
  required?: boolean;
  invalidClass?: string;
  /** Fired with the combined "+<dialCode> <number>" string on every change. */
  onChange?: (value: string) => void;
}

const PhoneInput: React.FC<PhoneInputProps> = ({
  id,
  name,
  defaultValue,
  placeholder,
  required,
  invalidClass,
  onChange,
}) => {
  const initial = useRef(splitPhone(defaultValue)).current;
  const dialRef = useRef<HTMLSelectElement>(null);
  const numberRef = useRef<HTMLInputElement>(null);
  const hiddenRef = useRef<HTMLInputElement>(null);

  const sync = () => {
    const dial = dialRef.current?.value ?? initial.dialCode;
    const number = numberRef.current?.value ?? '';
    const combined = number ? `${dial} ${number}` : '';
    if (hiddenRef.current) hiddenRef.current.value = combined;
    onChange?.(combined);
  };

  return (
    <div className="phone-input-group" style={{ display: 'flex', gap: 6 }}>
      <select
        ref={dialRef}
        aria-label="Country code"
        className="form-control form-select"
        style={{ flex: '0 0 auto', maxWidth: 130 }}
        defaultValue={initial.dialCode}
        onChange={sync}
      >
        {COUNTRY_CODES.map((c) => (
          <option key={c.iso} value={c.dialCode}>
            {c.flag} {c.dialCode}
          </option>
        ))}
      </select>
      <input
        ref={numberRef}
        id={id}
        type="tel"
        inputMode="numeric"
        className={`form-control${invalidClass ?? ''}`}
        placeholder={placeholder ?? 'Phone number'}
        required={required}
        defaultValue={initial.number}
        onChange={(e) => {
          e.currentTarget.value = e.currentTarget.value.replace(/\D/g, '');
          sync();
        }}
      />
      {name && (
        <input
          ref={hiddenRef}
          type="hidden"
          name={name}
          defaultValue={initial.number ? `${initial.dialCode} ${initial.number}` : ''}
        />
      )}
    </div>
  );
};

export default PhoneInput;
