import { useMemo, useState } from 'react';
import type { FormField, FormSchema } from '../../api/types';
import type { SubmitFormFiles, SubmitFormValues } from '../../api/forms';
import DynamicField from './DynamicField';
import { isFieldVisible, normalizeValues, type FieldValue } from './conditionEngine';
import { validateField } from './validateField';

export interface DynamicFormProps {
  schema: FormSchema;
  onSubmit: (values: SubmitFormValues, files: SubmitFormFiles) => Promise<void> | void;
  submitLabel?: string;
  /** Rendered above the sections - lets a caller (e.g. the builder's Preview) add a banner or heading. */
  banner?: React.ReactNode;
  formId?: string;
}

const allFieldsOf = (schema: FormSchema): FormField[] => schema.sections.flatMap((s) => s.fields);

/**
 * Renders a form from its schema and validates/submits it using the same engine everywhere the
 * schema is used: the real public form, and the admin builder's Preview.
 */
const DynamicForm: React.FC<DynamicFormProps> = ({ schema, onSubmit, submitLabel = 'Submit', banner, formId }) => {
  const fields = useMemo(() => allFieldsOf(schema), [schema]);
  const [values, setValues] = useState<Record<string, FieldValue>>({});
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  const normalized = normalizeValues(fields, values);
  const visibility = useMemo(() => {
    const map: Record<string, boolean> = {};
    for (const field of fields) {
      map[field.key] = isFieldVisible(field, normalized);
    }
    return map;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fields, JSON.stringify(normalized)]);

  const setValue = (key: string, value: FieldValue) => {
    setValues((prev) => ({ ...prev, [key]: value }));
    setErrors((prev) => {
      if (!prev[key]) return prev;
      const { [key]: _removed, ...next } = prev;
      return next;
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const nextErrors: Record<string, string> = {};
    for (const field of fields) {
      if (!visibility[field.key]) continue;
      const message = validateField(field, values[field.key]);
      if (message) nextErrors[field.key] = message;
    }

    if (Object.keys(nextErrors).length > 0) {
      setErrors(nextErrors);
      return;
    }

    const submitValues: SubmitFormValues = {};
    const submitFiles: SubmitFormFiles = {};

    for (const field of fields) {
      if (!visibility[field.key]) continue;
      const v = values[field.key];

      if (field.type === 'File' || field.type === 'MultiFile') {
        const list = Array.isArray(v) ? (v as File[]) : v instanceof File ? [v] : [];
        if (list.length > 0) submitFiles[field.key] = list;
        continue;
      }

      if (v !== undefined) {
        submitValues[field.key] = v as SubmitFormValues[string];
      }
    }

    setSubmitting(true);
    try {
      await onSubmit(submitValues, submitFiles);
    } catch {
      // the caller is responsible for surfacing the error (toast/banner) - just stop spinning here
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form id={formId} onSubmit={handleSubmit} noValidate>
      {banner}
      {schema.sections
        .filter((s) => s.visible && s.fields.some((f) => visibility[f.key]))
        .sort((a, b) => a.order - b.order)
        .map((section) => (
          <div key={section.key} className="mb-4">
            <h5 className="mb-3">{section.label}</h5>
            <div className="row">
              {section.fields
                .filter((f) => visibility[f.key])
                .sort((a, b) => a.order - b.order)
                .map((field) => (
                  <DynamicField
                    key={field.key}
                    field={field}
                    value={values[field.key]}
                    error={errors[field.key]}
                    onChange={(v) => setValue(field.key, v)}
                  />
                ))}
            </div>
          </div>
        ))}

      <div className="quote_submit_button">
        <button type="submit" className="btn btn-theme" disabled={submitting}>
          {submitting ? 'Submitting...' : submitLabel}
        </button>
      </div>
    </form>
  );
};

export default DynamicForm;
