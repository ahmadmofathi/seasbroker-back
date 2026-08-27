import { api, apiMultipart } from './client';
import { adminRequest } from './adminClient';
import type { FormSchema, FormSummary, SubmitFormResponse } from './types';

// ── Admin (Superuser) form-builder endpoints ──

export function listForms(): Promise<FormSummary[]> {
  return adminRequest<FormSummary[]>('/api/collections/forms/records');
}

export function getDraft(formKey: string): Promise<FormSchema> {
  return adminRequest<FormSchema>(`/api/collections/forms/records/${formKey}/draft`);
}

export function saveDraft(formKey: string, schema: FormSchema): Promise<FormSchema> {
  return adminRequest<FormSchema>(`/api/collections/forms/records/${formKey}/draft`, {
    method: 'PUT',
    body: schema,
  });
}

export function publishDraft(formKey: string): Promise<FormSchema> {
  return adminRequest<FormSchema>(`/api/collections/forms/records/${formKey}/publish`, {
    method: 'POST',
  });
}

// ── Public endpoints ──

export function getPublishedSchema(formKey: string): Promise<FormSchema> {
  return api<FormSchema>(`/api/forms/${formKey}/schema`);
}

/**
 * Non-file field values as a plain record (multiselect/checkbox-group values are string[]).
 * Files are keyed by field key; MultiFile fields may have more than one File per key.
 */
export type SubmitFormValues = Record<string, string | string[] | boolean | number | null | undefined>;
export type SubmitFormFiles = Record<string, File[]>;

export function submitForm(
  formKey: string,
  values: SubmitFormValues,
  files: SubmitFormFiles = {},
): Promise<SubmitFormResponse> {
  const formData = new FormData();
  formData.append('payload', JSON.stringify(values));

  for (const [fieldKey, fieldFiles] of Object.entries(files)) {
    for (const file of fieldFiles) {
      formData.append(`file:${fieldKey}`, file);
    }
  }

  return apiMultipart<SubmitFormResponse>(`/api/forms/${formKey}/submissions`, formData);
}
