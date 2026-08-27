import type { FormField } from '../../api/types';
import type { FieldValue } from './conditionEngine';

const FILE_TYPES = new Set(['File', 'MultiFile']);
const OPTION_TYPES = new Set(['Select', 'Radio']);

/** Client-side mirror of the backend's field validation, for immediate UX feedback. The backend
 * re-validates independently and is the source of truth - this never needs to be exhaustive. */
export function validateField(field: FormField, value: FieldValue): string | null {
  const isFile = FILE_TYPES.has(field.type);
  const files = isFile ? (Array.isArray(value) ? (value as File[]) : value instanceof File ? [value] : []) : [];
  const isEmpty = isFile
    ? files.length === 0
    : value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0);

  if (field.required && isEmpty) {
    return `${field.label} is required.`;
  }

  if (isEmpty) return null;

  const v = field.validation;

  if (OPTION_TYPES.has(field.type)) {
    if (!field.options.some((o) => o.value === value)) {
      return `${field.label} has an invalid selection.`;
    }
    return null;
  }

  if (field.type === 'MultiSelect') {
    const selected = value as string[];
    if (selected.some((s) => !field.options.some((o) => o.value === s))) {
      return `${field.label} has an invalid selection.`;
    }
    if (v?.minSelections != null && selected.length < v.minSelections) {
      return `${field.label} needs at least ${v.minSelections} selection(s).`;
    }
    if (v?.maxSelections != null && selected.length > v.maxSelections) {
      return `${field.label} allows at most ${v.maxSelections} selection(s).`;
    }
    return null;
  }

  if (field.type === 'Number' || field.type === 'Decimal') {
    const n = Number(value);
    if (Number.isNaN(n)) return `${field.label} must be a number.`;
    if (v?.min != null && n < v.min) return `${field.label} must be at least ${v.min}.`;
    if (v?.max != null && n > v.max) return `${field.label} must be at most ${v.max}.`;
    return null;
  }

  if (isFile) {
    for (const file of files) {
      if (v?.fileMaxSizeMB != null && file.size > v.fileMaxSizeMB * 1024 * 1024) {
        return `${field.label}: file '${file.name}' exceeds the ${v.fileMaxSizeMB} MB limit.`;
      }
      if (v?.allowedExtensions && v.allowedExtensions.length > 0) {
        const ext = file.name.split('.').pop()?.toLowerCase() ?? '';
        if (!v.allowedExtensions.some((a) => a.replace(/^\./, '').toLowerCase() === ext)) {
          return `${field.label}: file type '.${ext}' is not allowed.`;
        }
      }
    }
    return null;
  }

  // by this point value is one of the plain text/date field types, always a string at runtime
  const str = value as string;
  if (v?.minLength != null && str.length < v.minLength) {
    return `${field.label} must be at least ${v.minLength} characters.`;
  }
  if (v?.maxLength != null && str.length > v.maxLength) {
    return `${field.label} must be at most ${v.maxLength} characters.`;
  }
  if (v?.pattern) {
    try {
      if (!new RegExp(v.pattern).test(str)) {
        return `${field.label} is not in a valid format.`;
      }
    } catch {
      // malformed pattern - skip client-side check, backend still enforces it safely
    }
  }

  return null;
}
