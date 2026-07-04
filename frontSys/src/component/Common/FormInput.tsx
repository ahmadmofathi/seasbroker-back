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
  const [selectedValues, setSelectedValues] = useState<any>(multiSelect ? [] : "");
  const [searchTerm, setSearchTerm] = useState("");
  const [showDropdown, setShowDropdown] = useState(false);
  const dropdownRef = useRef<any>(null);

  const filteredOptions = options.filter((option) =>
    option.text.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleOptionSelect = (value: any) => {
    if (multiSelect) {
      if (!selectedValues.includes(value)) {
        setSelectedValues([...selectedValues, value]);
      }
    } else {
      setSelectedValues(value);
      setShowDropdown(false);
    }
    setSearchTerm("");
  };

  const removeSelectedValue = (value: any) => {
    if (multiSelect) {
      setSelectedValues(selectedValues.filter((v: any) => v !== value));
    }
  };

  useEffect(() => {
    const handleClickOutside = (event:any) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setShowDropdown(false);
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

      {tag === 'select' && (
        <div className="dropdown">
          <input
            type="text"
            placeholder={`Select ${label}`}
            className={classes}
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setShowDropdown(true);
            }}
            onClick={() => setShowDropdown(true)}
          />

          {showDropdown && (
            <div
              className="dropdown-menu show"
              style={{
                position: 'absolute',
                zIndex: 1000,
                width: '100%',
                maxHeight: '200px', // Set a max height for the dropdown
                overflowY: 'auto',  // Enable vertical scrolling
                border: '1px solid #ddd', // Optional: Add border for better visibility
                backgroundColor: '#fff', // Ensure it has a visible background
              }}
            >
              {filteredOptions.length > 0 ? (
                filteredOptions.map((option, index) => (
                  <div
                    key={index}
                    className="dropdown-item"
                    onClick={() => handleOptionSelect(option.value)}
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


          {multiSelect ? (
            <div className="selected-values">
              {selectedValues.map((value:any, index:any) => (
                <span
                  key={index}
                  className="badge badge-secondary"
                  style={{ marginRight: '5px', cursor: 'pointer' }}
                  onClick={() => removeSelectedValue(value)}
                >
                  {value} ×
                </span>
              ))}
            </div>
          ) : (
            <div className="selected-value">
              {selectedValues && <span className="badge badge-secondary">{selectedValues}</span>}
            </div>
          )}

          {/* Hidden input to include selected values in form submission */}
          <input
            type="hidden"
            name={name}
            value={multiSelect ? selectedValues.join(",") : selectedValues}
          />
        </div>
      )}
    </div>
  );
};

export default FormInput;
