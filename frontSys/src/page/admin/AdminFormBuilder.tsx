import { useCallback, useEffect, useState } from 'react';
import { getDraft, listForms, publishDraft, saveDraft } from '../../api/forms';
import type { FormSchema, FormSummary } from '../../api/types';
import { useAlert } from '../../context/AlertContext';
import { formatApiError } from '../../utils/formatApiError';
import BuilderCanvas from '../../component/admin/FormBuilder/BuilderCanvas';
import FieldConfigPanel from '../../component/admin/FormBuilder/FieldConfigPanel';
import PreviewModal from '../../component/admin/FormBuilder/PreviewModal';
import {
  addCondition,
  addField,
  addOption,
  addSection,
  changeFieldType,
  findField,
  moveField,
  moveOption,
  moveSection,
  removeCondition,
  removeField,
  removeOption,
  removeSection,
  updateCondition,
  updateField,
  updateOption,
  updateSection,
} from '../../component/admin/FormBuilder/builderOps';

const AdminFormBuilder: React.FC = () => {
  const { success, error: showError, confirm } = useAlert();

  const [forms, setForms] = useState<FormSummary[]>([]);
  const [selectedFormKey, setSelectedFormKey] = useState<string | null>(null);
  const [schema, setSchema] = useState<FormSchema | null>(null);
  const [selectedFieldKey, setSelectedFieldKey] = useState<string | null>(null);
  const [formsLoading, setFormsLoading] = useState(true);
  const [formsError, setFormsError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [publishing, setPublishing] = useState(false);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [scrollToSectionKey, setScrollToSectionKey] = useState<string | null>(null);

  const loadForms = useCallback(async () => {
    try {
      const items = await listForms();
      setForms(items);
      setFormsError(null);
      if (items.length > 0) {
        setSelectedFormKey((prev) => prev ?? items[0].key);
      }
    } catch (err) {
      const message = formatApiError(err);
      setFormsError(message);
      showError(message);
    } finally {
      setFormsLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    void loadForms();
  }, [loadForms]);

  useEffect(() => {
    if (!selectedFormKey) return;
    setLoading(true);
    setSelectedFieldKey(null);
    getDraft(selectedFormKey)
      .then(setSchema)
      .catch((err: unknown) => showError(formatApiError(err)))
      .finally(() => setLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedFormKey]);

  const activeSummary = forms.find((f) => f.key === selectedFormKey);
  const selectedField = schema && selectedFieldKey ? findField(schema, selectedFieldKey) : undefined;

  const apply = (fn: (s: FormSchema) => FormSchema) => setSchema((prev) => (prev ? fn(prev) : prev));

  const handleSaveDraft = async () => {
    if (!selectedFormKey || !schema) return;
    setSaving(true);
    try {
      const saved = await saveDraft(selectedFormKey, schema);
      setSchema(saved);
      await loadForms();
      success('Draft saved.');
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  };

  const handlePublish = async () => {
    if (!selectedFormKey) return;
    const ok = await confirm({
      title: 'Publish this form?',
      message: 'The published version will become live for all visitors immediately.',
      confirmText: 'Publish',
      variant: 'primary',
    });
    if (!ok) return;

    setPublishing(true);
    try {
      await handleSaveDraft();
      const published = await publishDraft(selectedFormKey);
      setSchema(published);
      await loadForms();
      success('Form published.');
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setPublishing(false);
    }
  };

  return (
    <div className="fb-page">
      <div className="fb-toolbar">
        <div className="admin-action-bar">
          {forms.map((f) => (
            <button
              key={f.key}
              type="button"
              className={`admin-btn-sm ${selectedFormKey === f.key ? 'primary' : 'outline'}`}
              onClick={() => setSelectedFormKey(f.key)}
            >
              {f.name}
            </button>
          ))}
        </div>

        {activeSummary && (
          <div className="admin-action-bar">
            <span className="admin-badge">
              Published v{activeSummary.publishedVersionNumber ?? '-'}
            </span>
            {activeSummary.hasUnpublishedDraft && (
              <span className="admin-badge" style={{ background: '#fffbeb', color: '#92400e' }}>
                Draft v{activeSummary.draftVersionNumber} (unpublished)
              </span>
            )}
            {selectedField && (
              <span className="admin-badge" style={{ background: '#eef2ff', color: 'var(--admin-navy)' }}>
                Editing: {selectedField.label}
              </span>
            )}
            <button type="button" className="admin-btn-sm outline" onClick={() => setPreviewOpen(true)} disabled={!schema}>
              <i className="ri-eye-line" /> Preview
            </button>
            <button type="button" className="admin-btn-sm outline" onClick={() => void handleSaveDraft()} disabled={saving || !schema}>
              <i className="ri-save-line" /> {saving ? 'Saving...' : 'Save Draft'}
            </button>
            <button type="button" className="admin-btn-sm primary" onClick={() => void handlePublish()} disabled={publishing || !schema}>
              <i className="ri-upload-cloud-line" /> {publishing ? 'Publishing...' : 'Publish'}
            </button>
          </div>
        )}
      </div>

      {formsLoading && <div className="admin-loading"><div className="admin-spinner" /> Loading forms...</div>}

      {!formsLoading && formsError && forms.length === 0 && (
        <div className="admin-alert-error">
          <i className="ri-error-warning-line" /> {formsError}
          <button type="button" className="admin-btn-sm outline" style={{ marginLeft: '0.75rem' }} onClick={() => { setFormsLoading(true); void loadForms(); }}>
            Retry
          </button>
        </div>
      )}

      {!formsLoading && loading && <div className="admin-loading"><div className="admin-spinner" /> Loading form...</div>}

      {!formsLoading && !loading && schema && (
        <div className={`fb-columns${selectedField ? '' : ' fb-columns-single'}`}>
          <div className="fb-pane">
            <BuilderCanvas
              schema={schema}
              selectedFieldKey={selectedFieldKey}
              onSelectField={setSelectedFieldKey}
              onAddSection={() => {
                const key = `section-${crypto.randomUUID().slice(0, 8)}`;
                apply((s) => addSection(s, key));
                setScrollToSectionKey(key);
              }}
              onRemoveSection={(key) => apply((s) => removeSection(s, key))}
              onMoveSection={(key, dir) => apply((s) => moveSection(s, key, dir))}
              onRenameSection={(key, label) => apply((s) => updateSection(s, key, { label }))}
              onToggleSectionVisible={(key, visible) => apply((s) => updateSection(s, key, { visible }))}
              onAddField={(sectionKey) => {
                const key = `custom-${crypto.randomUUID().slice(0, 8)}`;
                apply((s) => addField(s, sectionKey, key));
                setSelectedFieldKey(key);
              }}
              onRemoveField={(key) => {
                apply((s) => removeField(s, key));
                if (selectedFieldKey === key) setSelectedFieldKey(null);
              }}
              onMoveField={(key, dir) => apply((s) => moveField(s, key, dir))}
              scrollToSectionKey={scrollToSectionKey}
              onScrolledToSection={() => setScrollToSectionKey(null)}
            />
          </div>

          {selectedField && schema && (
            <div className="fb-sheet-overlay" role="presentation" onClick={() => setSelectedFieldKey(null)}>
              <div className="fb-sheet" onClick={(e) => e.stopPropagation()}>
                <FieldConfigPanel
                  schema={schema}
                  field={selectedField}
                  onUpdateField={(patch) => apply((s) => updateField(s, selectedField.key, patch))}
                  onChangeType={(type) => apply((s) => changeFieldType(s, selectedField.key, type))}
                  onAddOption={() => apply((s) => addOption(s, selectedField.key))}
                  onUpdateOption={(index, patch) => apply((s) => updateOption(s, selectedField.key, index, patch))}
                  onRemoveOption={(index) => apply((s) => removeOption(s, selectedField.key, index))}
                  onMoveOption={(index, dir) => apply((s) => moveOption(s, selectedField.key, index, dir))}
                  onAddCondition={() => apply((s) => addCondition(s, selectedField.key))}
                  onUpdateCondition={(index, patch) => apply((s) => updateCondition(s, selectedField.key, index, patch))}
                  onRemoveCondition={(index) => apply((s) => removeCondition(s, selectedField.key, index))}
                  onClose={() => setSelectedFieldKey(null)}
                />
              </div>
            </div>
          )}
        </div>
      )}

      {previewOpen && schema && <PreviewModal schema={schema} onClose={() => setPreviewOpen(false)} />}
    </div>
  );
};

export default AdminFormBuilder;
