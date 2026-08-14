import { useState, useRef, useEffect } from 'react';

interface FormInputProps {
  tag: 'input' | 'textarea' | 'button' | 'select';
  name: string;
  type?: string;
  placeholder?: string;
  classes?: string;
  label?: string;
  options?: { value: string; text: string }[];
  multiSelect?: boolean;
  val?: string; // For button tag
}

const FormInput: React.FC<FormInputProps> = ({ tag, name, type, placeholder, classes, label, options = [], multiSelect = false, val }) => {
  const [selectedValues, setSelectedValues] = useState<string[]>([]);
  const [singleValue, setSingleValue] = useState<string>("");
  const dropdownRef = useRef<any>(null);

  const handleOptionSelect = (value: string) => {
    if (multiSelect) {
      if (!selectedValues.includes(value)) {
        setSelectedValues((prev) => [...prev, value]);
      }
    } else {
      setSingleValue(value);
    }
  };

  const removeSelectedValue = (value: string) => {
    if (multiSelect) {
      setSelectedValues((prev) => prev.filter((v) => v !== value));
    }
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        // no-op: the simplified select variant does not use a custom popup list
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <div className="form-group" ref={dropdownRef}>
      {label && <label htmlFor={name}>{label}</label>}

      {tag === 'input' && (
        <input
          type={type}
          name={name}
          placeholder={placeholder}
          className={classes}
        />
      )}

      {tag === 'textarea' && (
        <textarea
          name={name}
          cols={30}
          rows={7}
          placeholder={placeholder}
          className={classes}
          required
        />
      )}
      {tag === 'button' && (
        <button className={`btn btn-theme`}>{val}</button>
      )}

      {tag === 'select' && !multiSelect && (
        <select
          name={name}
          className={classes}
          value={singleValue}
          onChange={(e) => setSingleValue(e.target.value)}
        >
          <option value="">{placeholder || `Select ${label ?? name}`}</option>
          {options.map((option, index) => (
            <option key={`${option.value}-${index}`} value={option.value}>
              {option.text}
            </option>
          ))}
        </select>
      )}

      {tag === 'select' && multiSelect && (
        <div className="dropdown">
          <select
            name={`${name}-selector`}
            className={classes}
            value=""
            onChange={(e) => {
              const value = e.target.value;
              if (value) {
                handleOptionSelect(value);
                e.currentTarget.value = '';
              }
            }}
          >
            <option value="">{placeholder || `Select ${label ?? name}`}</option>
            {options.map((option, index) => (
              <option key={`${option.value}-${index}`} value={option.value}>
                {option.text}
              </option>
            ))}
          </select>

          <div className="selected-values">
            {selectedValues.map((value, index) => (
              <span
                key={`${value}-${index}`}
                className="badge badge-secondary"
                style={{ marginRight: '5px', cursor: 'pointer' }}
                onClick={() => removeSelectedValue(value)}
              >
                {value} ×
              </span>
            ))}
          </div>

          <input
            type="hidden"
            name={name}
            value={selectedValues.join(',')}
          />
        </div>
      )}
    </div>
  );
};

export default FormInput;
