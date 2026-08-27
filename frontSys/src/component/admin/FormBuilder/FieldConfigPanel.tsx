import type {
  FormConditionOperator,
  FormField,
  FormFieldCondition,
  FormFieldOption,
  FormFieldType,
  FormFieldValidation,
  FormFieldWidth,
  FormSchema,
} from '../../../api/types';
import { allFieldsExcept } from './builderOps';

interface FieldConfigPanelProps {
  schema: FormSchema;
  field: FormField;
  onUpdateField: (patch: Partial<FormField>) => void;
  onChangeType: (type: FormFieldType) => void;
  onAddOption: () => void;
  onUpdateOption: (index: number, patch: Partial<FormFieldOption>) => void;
  onRemoveOption: (index: number) => void;
  onMoveOption: (index: number, direction: -1 | 1) => void;
  onAddCondition: () => void;
  onUpdateCondition: (index: number, patch: Partial<FormFieldCondition>) => void;
  onRemoveCondition: (index: number) => void;
  onClose: () => void;
}

const FIELD_TYPES: FormFieldType[] = [
  'Text', 'Textarea', 'Number', 'Decimal', 'Date', 'DateTime', 'Time', 'Email', 'Phone',
  'Select', 'MultiSelect', 'Radio', 'Checkbox', 'Toggle', 'File', 'MultiFile',
];

const OPERATORS: { value: FormConditionOperator; label: string }[] = [
  { value: 'Equals', label: 'is' },
  { value: 'NotEquals', label: 'is not' },
  { value: 'Contains', label: 'contains' },
  { value: 'GreaterThan', label: '>' },
  { value: 'GreaterThanOrEqual', label: '>=' },
  { value: 'LessThan', label: '<' },
  { value: 'LessThanOrEqual', label: '<=' },
  { value: 'IsEmpty', label: 'is empty' },
  { value: 'IsNotEmpty', label: 'is not empty' },
  { value: 'In', label: 'is one of' },
  { value: 'NotIn', label: 'is not one of' },
];

const NO_VALUE_OPERATORS = new Set<FormConditionOperator>(['IsEmpty', 'IsNotEmpty']);
const LIST_OPERATORS = new Set<FormConditionOperator>(['In', 'NotIn']);
const OPTION_TYPES = new Set<FormFieldType>(['Select', 'MultiSelect', 'Radio']);

const toCsv = (jsonValue?: string | null): string => {
  if (!jsonValue) return '';
  try {
    const parsed: unknown = JSON.parse(jsonValue);
    return Array.isArray(parsed) ? parsed.join(', ') : jsonValue;
  } catch {
    return jsonValue;
  }
};

const fromCsv = (csv: string): string => JSON.stringify(csv.split(',').map((v) => v.trim()).filter(Boolean));

const setValidation = (field: FormField, patch: Partial<FormFieldValidation>): FormFieldValidation => ({
  ...field.validation,
  ...patch,
});

const FieldConfigPanel: React.FC<FieldConfigPanelProps> = ({
  schema,
  field,
  onUpdateField,
  onChangeType,
  onAddOption,
  onUpdateOption,
  onRemoveOption,
  onMoveOption,
  onAddCondition,
  onUpdateCondition,
  onRemoveCondition,
  onClose,
}) => {
  const candidateSourceFields = allFieldsExcept(schema, field.key);
  const v = field.validation ?? {};

  return (
    <div className="admin-panel">
      <div className="admin-panel-header">
        <h2>Field Settings</h2>
        <button type="button" className="admin-btn-sm outline" onClick={onClose}>
          <i className="ri-close-line" /> Close
        </button>
      </div>
      <div className="admin-panel-body">
        <div className="admin-form-grid">
          <div className="admin-field full">
            <label>Label</label>
            <input
              className="admin-input"
              value={field.label}
              onChange={(e) => onUpdateField({ label: e.target.value })}
            />
          </div>

          <div className="admin-field">
            <label>Field Key</label>
            <input className="admin-input" value={field.key} disabled title="Stable key - never changes" />
          </div>

          <div className="admin-field">
            <label>Type</label>
            <select
              className="admin-input"
              value={field.type}
              disabled={field.isSystemField}
              onChange={(e) => onChangeType(e.target.value as FormFieldType)}
            >
              {FIELD_TYPES.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </div>

          <div className="admin-field full">
            <label>Placeholder</label>
            <input
              className="admin-input"
              value={field.placeholder ?? ''}
              onChange={(e) => onUpdateField({ placeholder: e.target.value })}
            />
          </div>

          <div className="admin-field full">
            <label>Help Text</label>
            <input
              className="admin-input"
              value={field.helpText ?? ''}
              onChange={(e) => onUpdateField({ helpText: e.target.value })}
            />
          </div>

          <div className="admin-field">
            <label>Width</label>
            <select
              className="admin-input"
              value={field.width}
              onChange={(e) => onUpdateField({ width: e.target.value as FormFieldWidth })}
            >
              <option value="Full">Full width</option>
              <option value="Half">Half width</option>
              <option value="Third">Third width</option>
            </select>
          </div>

          <div className="admin-field" style={{ display: 'flex', alignItems: 'flex-end', gap: '1rem' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <input
                type="checkbox"
                checked={field.required}
                onChange={(e) => onUpdateField({ required: e.target.checked })}
              />
              Required
            </label>
            <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <input
                type="checkbox"
                checked={field.visible}
                onChange={(e) => onUpdateField({ visible: e.target.checked })}
              />
              Visible
            </label>
          </div>
        </div>

        {OPTION_TYPES.has(field.type) && (
          <div style={{ marginTop: '1.25rem' }}>
            <h3 style={{ fontSize: '0.9rem', color: 'var(--admin-navy)', marginBottom: '0.5rem' }}>Options</h3>
            {field.options.map((option, index) => (
              <div key={index} style={{ display: 'flex', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <input
                  className="admin-input"
                  placeholder="Label"
                  value={option.label}
                  onChange={(e) => onUpdateOption(index, { label: e.target.value })}
                />
                <input
                  className="admin-input"
                  placeholder="Value"
                  value={option.value}
                  onChange={(e) => onUpdateOption(index, { value: e.target.value })}
                />
                <button type="button" className="admin-btn-sm outline" disabled={index === 0} onClick={() => onMoveOption(index, -1)}>
                  <i className="ri-arrow-up-line" />
                </button>
                <button
                  type="button"
                  className="admin-btn-sm outline"
                  disabled={index === field.options.length - 1}
                  onClick={() => onMoveOption(index, 1)}
                >
                  <i className="ri-arrow-down-line" />
                </button>
                <button type="button" className="admin-btn-sm danger" onClick={() => onRemoveOption(index)}>
                  <i className="ri-delete-bin-line" />
                </button>
              </div>
            ))}
            <button type="button" className="admin-btn-sm outline" onClick={onAddOption}>
              <i className="ri-add-line" /> Add Option
            </button>
          </div>
        )}

        <div style={{ marginTop: '1.25rem' }}>
          <h3 style={{ fontSize: '0.9rem', color: 'var(--admin-navy)', marginBottom: '0.5rem' }}>Validation</h3>
          <div className="admin-form-grid">
            {['Text', 'Textarea', 'Email', 'Phone'].includes(field.type) && (
              <>
                <div className="admin-field">
                  <label>Min Length</label>
                  <input
                    type="number"
                    className="admin-input"
                    value={v.minLength ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { minLength: e.target.value === '' ? null : Number(e.target.value) }) })}
                  />
                </div>
                <div className="admin-field">
                  <label>Max Length</label>
                  <input
                    type="number"
                    className="admin-input"
                    value={v.maxLength ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { maxLength: e.target.value === '' ? null : Number(e.target.value) }) })}
                  />
                </div>
                <div className="admin-field full">
                  <label>Pattern (regex, optional)</label>
                  <input
                    className="admin-input"
                    value={v.pattern ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { pattern: e.target.value || null }) })}
                  />
                </div>
              </>
            )}

            {['Number', 'Decimal'].includes(field.type) && (
              <>
                <div className="admin-field">
                  <label>Min</label>
                  <input
                    type="number"
                    className="admin-input"
                    value={v.min ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { min: e.target.value === '' ? null : Number(e.target.value) }) })}
                  />
                </div>
                <div className="admin-field">
                  <label>Max</label>
                  <input
                    type="number"
                    className="admin-input"
                    value={v.max ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { max: e.target.value === '' ? null : Number(e.target.value) }) })}
                  />
                </div>
              </>
            )}

            {field.type === 'MultiSelect' && (
              <>
                <div className="admin-field">
                  <label>Min Selections</label>
                  <input
                    type="number"
                    className="admin-input"
                    value={v.minSelections ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { minSelections: e.target.value === '' ? null : Number(e.target.value) }) })}
                  />
                </div>
                <div className="admin-field">
                  <label>Max Selections</label>
                  <input
                    type="number"
                    className="admin-input"
                    value={v.maxSelections ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { maxSelections: e.target.value === '' ? null : Number(e.target.value) }) })}
                  />
                </div>
              </>
            )}

            {['File', 'MultiFile'].includes(field.type) && (
              <>
                <div className="admin-field">
                  <label>Max File Size (MB)</label>
                  <input
                    type="number"
                    className="admin-input"
                    value={v.fileMaxSizeMB ?? ''}
                    onChange={(e) => onUpdateField({ validation: setValidation(field, { fileMaxSizeMB: e.target.value === '' ? null : Number(e.target.value) }) })}
                  />
                </div>
                <div className="admin-field">
                  <label>Allowed Extensions (comma-separated)</label>
                  <input
                    className="admin-input"
                    value={(v.allowedExtensions ?? []).join(', ')}
                    onChange={(e) =>
                      onUpdateField({
                        validation: setValidation(field, {
                          allowedExtensions: e.target.value.split(',').map((s) => s.trim()).filter(Boolean),
                        }),
                      })
                    }
                  />
                </div>
              </>
            )}
          </div>
        </div>

        <div style={{ marginTop: '1.25rem' }}>
          <h3 style={{ fontSize: '0.9rem', color: 'var(--admin-navy)', marginBottom: '0.5rem' }}>Conditional Visibility</h3>
          <p className="admin-page-desc" style={{ display: 'block' }}>Show this field when:</p>

          {field.conditions.length > 1 && (
            <div className="admin-field" style={{ maxWidth: 260 }}>
              <label>Combine conditions with</label>
              <select
                className="admin-input"
                value={field.conditionCombinator ?? 'AND'}
                onChange={(e) => onUpdateField({ conditionCombinator: e.target.value as 'AND' | 'OR' })}
              >
                <option value="AND">All conditions match (AND)</option>
                <option value="OR">Any condition matches (OR)</option>
              </select>
            </div>
          )}

          {field.conditions.map((condition, index) => (
            <div key={index} style={{ display: 'flex', gap: '0.5rem', marginBottom: '0.5rem', flexWrap: 'wrap', alignItems: 'center' }}>
              <select
                className="admin-input"
                style={{ flex: '1 1 180px' }}
                value={condition.sourceFieldKey}
                onChange={(e) => onUpdateCondition(index, { sourceFieldKey: e.target.value })}
              >
                <option value="">Select a field...</option>
                {candidateSourceFields.map((f) => (
                  <option key={f.key} value={f.key}>{f.label}</option>
                ))}
              </select>
              <select
                className="admin-input"
                style={{ flex: '1 1 140px' }}
                value={condition.operator}
                onChange={(e) => onUpdateCondition(index, { operator: e.target.value as FormConditionOperator })}
              >
                {OPERATORS.map((op) => (
                  <option key={op.value} value={op.value}>{op.label}</option>
                ))}
              </select>
              {!NO_VALUE_OPERATORS.has(condition.operator) && (
                <input
                  className="admin-input"
                  style={{ flex: '1 1 180px' }}
                  placeholder={LIST_OPERATORS.has(condition.operator) ? 'value1, value2, ...' : 'Value'}
                  value={LIST_OPERATORS.has(condition.operator) ? toCsv(condition.value) : condition.value ?? ''}
                  onChange={(e) =>
                    onUpdateCondition(index, {
                      value: LIST_OPERATORS.has(condition.operator) ? fromCsv(e.target.value) : e.target.value,
                    })
                  }
                />
              )}
              <button type="button" className="admin-btn-sm danger" onClick={() => onRemoveCondition(index)}>
                <i className="ri-delete-bin-line" />
              </button>
            </div>
          ))}

          <button type="button" className="admin-btn-sm outline" onClick={onAddCondition} disabled={candidateSourceFields.length === 0}>
            <i className="ri-add-line" /> Add Condition
          </button>
        </div>
      </div>
    </div>
  );
};

export default FieldConfigPanel;
