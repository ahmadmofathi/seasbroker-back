import { useEffect, useRef } from 'react';
import type { FormSchema } from '../../../api/types';

interface BuilderCanvasProps {
  schema: FormSchema;
  selectedFieldKey: string | null;
  onSelectField: (fieldKey: string) => void;
  onAddSection: () => void;
  onRemoveSection: (sectionKey: string) => void;
  onMoveSection: (sectionKey: string, direction: -1 | 1) => void;
  onRenameSection: (sectionKey: string, label: string) => void;
  onToggleSectionVisible: (sectionKey: string, visible: boolean) => void;
  onAddField: (sectionKey: string) => void;
  onRemoveField: (fieldKey: string) => void;
  onMoveField: (fieldKey: string, direction: -1 | 1) => void;
  /** Section to scroll into view once it renders (e.g. right after it's added). */
  scrollToSectionKey?: string | null;
  onScrolledToSection?: () => void;
}

const BuilderCanvas: React.FC<BuilderCanvasProps> = ({
  schema,
  selectedFieldKey,
  onSelectField,
  onAddSection,
  onRemoveSection,
  onMoveSection,
  onRenameSection,
  onToggleSectionVisible,
  onAddField,
  onRemoveField,
  onMoveField,
  scrollToSectionKey,
  onScrolledToSection,
}) => {
  const sections = [...schema.sections].sort((a, b) => a.order - b.order);
  const sectionRefs = useRef<Record<string, HTMLDivElement | null>>({});

  useEffect(() => {
    if (!scrollToSectionKey) return;
    const node = sectionRefs.current[scrollToSectionKey];
    if (node) {
      node.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
    onScrolledToSection?.();
  }, [scrollToSectionKey, onScrolledToSection]);

  return (
    <div className="admin-panel">
      <div className="admin-panel-header">
        <h2>Form Structure</h2>
        <button type="button" className="admin-btn-sm primary" onClick={onAddSection}>
          <i className="ri-add-line" /> Add Section
        </button>
      </div>
      <div className="admin-panel-body">
        {sections.length === 0 && <div className="admin-empty">No sections yet. Add one to get started.</div>}

        {sections.map((section, sectionIdx) => {
          const fields = [...section.fields].sort((a, b) => a.order - b.order);
          return (
            <div
              key={section.key}
              ref={(el) => {
                sectionRefs.current[section.key] = el;
              }}
              className="fb-section"
              style={{ marginBottom: '1.25rem' }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <input
                  className="admin-input"
                  style={{ fontWeight: 600, flex: 1 }}
                  value={section.label}
                  onChange={(e) => onRenameSection(section.key, e.target.value)}
                />
                <label className="admin-badge" style={{ display: 'flex', alignItems: 'center', gap: 4, cursor: 'pointer' }}>
                  <input
                    type="checkbox"
                    checked={section.visible}
                    onChange={(e) => onToggleSectionVisible(section.key, e.target.checked)}
                  />
                  Visible
                </label>
                <button
                  type="button"
                  className="admin-btn-sm outline"
                  disabled={sectionIdx === 0}
                  onClick={() => onMoveSection(section.key, -1)}
                  aria-label="Move section up"
                >
                  <i className="ri-arrow-up-line" />
                </button>
                <button
                  type="button"
                  className="admin-btn-sm outline"
                  disabled={sectionIdx === sections.length - 1}
                  onClick={() => onMoveSection(section.key, 1)}
                  aria-label="Move section down"
                >
                  <i className="ri-arrow-down-line" />
                </button>
                <button type="button" className="admin-btn-sm danger" onClick={() => onRemoveSection(section.key)}>
                  <i className="ri-delete-bin-line" />
                </button>
              </div>

              <div className="admin-table-wrap">
                <table className="admin-table">
                  <tbody>
                    {fields.map((field, fieldIdx) => (
                      <tr
                        key={field.key}
                        onClick={() => onSelectField(field.key)}
                        className={`fb-field-row${selectedFieldKey === field.key ? ' selected' : ''}`}
                        aria-selected={selectedFieldKey === field.key}
                      >
                        <td style={{ width: '40%' }}>
                          {selectedFieldKey === field.key && <i className="ri-checkbox-blank-circle-fill" style={{ fontSize: '0.5rem', color: 'var(--admin-red)', marginRight: 6 }} />}
                          {field.label}
                          {!field.visible && <span className="admin-badge" style={{ marginLeft: 6 }}>Hidden</span>}
                          {field.required && <span className="admin-badge" style={{ marginLeft: 6 }}>Required</span>}
                          {field.isSystemField && <span className="admin-badge" style={{ marginLeft: 6 }}>System</span>}
                          {field.conditions.length > 0 && (
                            <span className="admin-badge" style={{ marginLeft: 6 }}>Conditional</span>
                          )}
                        </td>
                        <td>{field.type}</td>
                        <td className="admin-actions-cell" onClick={(e) => e.stopPropagation()}>
                          <button
                            type="button"
                            className="admin-btn-sm outline"
                            disabled={fieldIdx === 0}
                            onClick={() => onMoveField(field.key, -1)}
                            aria-label="Move field up"
                          >
                            <i className="ri-arrow-up-line" />
                          </button>
                          <button
                            type="button"
                            className="admin-btn-sm outline"
                            disabled={fieldIdx === fields.length - 1}
                            onClick={() => onMoveField(field.key, 1)}
                            aria-label="Move field down"
                          >
                            <i className="ri-arrow-down-line" />
                          </button>
                          {!field.isSystemField && (
                            <button
                              type="button"
                              className="admin-btn-sm danger"
                              onClick={() => onRemoveField(field.key)}
                            >
                              <i className="ri-delete-bin-line" />
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                    {fields.length === 0 && (
                      <tr>
                        <td colSpan={3} className="admin-empty" style={{ padding: '1rem' }}>
                          No fields in this section.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>

              <button
                type="button"
                className="admin-btn-sm outline"
                style={{ marginTop: '0.5rem' }}
                onClick={() => onAddField(section.key)}
              >
                <i className="ri-add-line" /> Add Field
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default BuilderCanvas;
