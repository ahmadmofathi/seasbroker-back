import type { FormField, FormFieldCondition } from '../../api/types';

/**
 * Mirrors the backend's ConditionEvaluator exactly (same operators, same semantics) so a field
 * that's visible on screen is guaranteed to be accepted (or rejected) the same way once submitted.
 */
export function isFieldVisible(field: FormField, normalizedValues: Record<string, string>): boolean {
  if (!field.visible) return false;
  if (field.conditions.length === 0) return true;

  const results = field.conditions.map((c) => evaluateCondition(c, normalizedValues));
  return field.conditionCombinator === 'OR' ? results.some(Boolean) : results.every(Boolean);
}

function evaluateCondition(condition: FormFieldCondition, values: Record<string, string>): boolean {
  const actual = (values[condition.sourceFieldKey] ?? '').trim();
  const expected = condition.value ?? '';

  switch (condition.operator) {
    case 'IsEmpty':
      return actual === '' || actual === '[]';
    case 'IsNotEmpty':
      return actual !== '' && actual !== '[]';
    case 'Equals':
      return actual.toLowerCase() === expected.toLowerCase();
    case 'NotEquals':
      return actual.toLowerCase() !== expected.toLowerCase();
    case 'Contains':
      return (
        toList(actual).some((v) => v.toLowerCase() === expected.toLowerCase()) ||
        actual.toLowerCase().includes(expected.toLowerCase())
      );
    case 'In':
      return toList(expected).some((v) => v.toLowerCase() === actual.toLowerCase());
    case 'NotIn':
      return !toList(expected).some((v) => v.toLowerCase() === actual.toLowerCase());
    case 'GreaterThan':
      return compare(actual, expected) > 0;
    case 'GreaterThanOrEqual':
      return compare(actual, expected) >= 0;
    case 'LessThan':
      return compare(actual, expected) < 0;
    case 'LessThanOrEqual':
      return compare(actual, expected) <= 0;
    default:
      return false;
  }
}

function compare(a: string, b: string): number {
  const na = Number(a);
  const nb = Number(b);
  if (a !== '' && b !== '' && !Number.isNaN(na) && !Number.isNaN(nb)) {
    return na - nb;
  }

  const da = Date.parse(a);
  const db = Date.parse(b);
  if (!Number.isNaN(da) && !Number.isNaN(db)) {
    return da - db;
  }

  return a < b ? -1 : a > b ? 1 : 0;
}

function toList(value?: string | null): string[] {
  if (!value) return [];
  const trimmed = value.trim();
  if (trimmed.startsWith('[')) {
    try {
      const parsed: unknown = JSON.parse(trimmed);
      return Array.isArray(parsed) ? parsed.map(String) : [];
    } catch {
      return [];
    }
  }
  return trimmed.split(',').map((v) => v.trim()).filter(Boolean);
}

export type FieldValue = string | string[] | boolean | File | File[] | null | undefined;

/** Reduces the raw per-field value state to the flat string map conditions are evaluated against. */
export function normalizeValues(fields: FormField[], values: Record<string, FieldValue>): Record<string, string> {
  const out: Record<string, string> = {};
  for (const field of fields) {
    const v = values[field.key];
    out[field.key] = normalizeOne(v);
  }
  return out;
}

function normalizeOne(v: FieldValue): string {
  if (v === null || v === undefined) return '';
  if (Array.isArray(v)) {
    if (v.length > 0 && v[0] instanceof File) {
      return (v as File[]).map((f) => f.name).join(', ');
    }
    return JSON.stringify(v);
  }
  if (typeof v === 'boolean') return v ? 'true' : 'false';
  if (v instanceof File) return v.name;
  return String(v);
}
