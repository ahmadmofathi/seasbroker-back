import { useEffect, useMemo, useRef, useState, type JSX, type RefObject } from 'react';

interface Option { value: string; text: string }

interface FormSelectProps<T extends object> {
  label?: string;
  placeholder?: string;
  options: Option[];
  formField: Extract<keyof T, string>;
  error?: string;
  formData: T;
  setFormData: React.Dispatch<React.SetStateAction<T>>;
}

const FormSelect = <T extends object>({
  label,
  placeholder,
  options,
  formField,
  error,
  formData,
  setFormData
}: FormSelectProps<T>): JSX.Element => {
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [showDropdown, setShowDropdown] = useState<boolean>(false);
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState<string>("");
  const dropdownRef = useRef<HTMLDivElement>(null);

  const filteredOptions = useMemo(() => {
    return options.filter(option =>
      option.text.toLowerCase().includes(debouncedSearchTerm.toLowerCase())
    );
  }, [options, debouncedSearchTerm]);

  useEffect(() => { // Handle clicks outside the dropdown to close it
    const isNotFocused = (ref: RefObject<HTMLDivElement | null>, event: MouseEvent) => {
      return ref.current && !ref.current.contains(event.target as Node);
    };

    const handleClickOutside = (event: MouseEvent) => {
      if (isNotFocused(dropdownRef, event)) {
        setShowDropdown(false);
        if (filteredOptions.length == 0) {
          setSearchTerm("");
        }
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  });

  useEffect(() => { // Debounce the search term to avoid excessive filtering
    const handler = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 300);

    return () => {
      clearTimeout(handler);
    };
  }, [searchTerm]);


  const handleOptionSelect = (value: string) => {
    setFormData({ ...formData, [formField]: value });
    setShowDropdown(false);
    setSearchTerm(value);
  };

  return (
    <div className="form-group" ref={dropdownRef}>
      {label && <label htmlFor={formField} className="form-label">{label}</label>}
      <div className="dropdown">
        <input
          type="text"
          className={`form-control${error !== "" ? ' is-invalid' : ''}`}
          placeholder={placeholder || `Select ${formField}`}
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            if (formData[formField] !== "") {
              setFormData({ ...formData, [formField]: "" });
            }
          }}
          onClick={() => { setShowDropdown(true) }}
        />
        {showDropdown && (
          <div
            className="dropdown-menu show"
            style={{
              position: 'absolute',
              zIndex: 1000,
              width: '100%',
              maxHeight: '200px',
              overflowY: 'auto',
              border: '1px solid #ddd',
              backgroundColor: '#fff',
            }}
          >
            {filteredOptions.length > 0 ? (
              filteredOptions.map((option, index) => (
                <div
                  key={index}
                  className="dropdown-item"
                  onClick={() => { handleOptionSelect(option.value) }}
                  style={{ cursor: 'pointer' }}
                >
                  {option.text}
                </div>
              ))
            ) : (
              <div className="dropdown-item">No results found</div>
            )}
          </div>
        )}
        {error !== "" && <div className="invalid-feedback">{error}</div>}
      </div>
    </div>
  );
};

export default FormSelect;
