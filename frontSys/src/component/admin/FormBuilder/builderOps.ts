import type {
  FormField,
  FormFieldCondition,
  FormFieldOption,
  FormFieldType,
  FormSchema,
  FormSection,
} from '../../../api/types';

const newKey = (prefix: string) => `${prefix}-${crypto.randomUUID().slice(0, 8)}`;

export function emptySection(order: number, key: string = newKey('section')): FormSection {
  return { key, label: 'New Section', order, visible: true, fields: [] };
}

export function emptyField(order: number, key: string = newKey('custom')): FormField {
  return {
    key,
    label: 'New Field',
    type: 'Text',
    required: false,
    visible: true,
    order,
    width: 'Full',
    isSystemField: false,
    options: [],
    conditions: [],
  };
}

function mapSection(schema: FormSchema, sectionKey: string, fn: (s: FormSection) => FormSection): FormSchema {
  return { ...schema, sections: schema.sections.map((s) => (s.key === sectionKey ? fn(s) : s)) };
}

function mapField(schema: FormSchema, fieldKey: string, fn: (f: FormField) => FormField): FormSchema {
  return {
    ...schema,
    sections: schema.sections.map((s) => ({
      ...s,
      fields: s.fields.map((f) => (f.key === fieldKey ? fn(f) : f)),
    })),
  };
}

export function findField(schema: FormSchema, fieldKey: string): FormField | undefined {
  for (const section of schema.sections) {
    const found = section.fields.find((f) => f.key === fieldKey);
    if (found) return found;
  }
  return undefined;
}

export function findFieldSection(schema: FormSchema, fieldKey: string): FormSection | undefined {
  return schema.sections.find((s) => s.fields.some((f) => f.key === fieldKey));
}

export function addSection(schema: FormSchema, key?: string): FormSchema {
  return { ...schema, sections: [...schema.sections, emptySection(schema.sections.length, key)] };
}

export function removeSection(schema: FormSchema, sectionKey: string): FormSchema {
  return { ...schema, sections: schema.sections.filter((s) => s.key !== sectionKey) };
}

export function moveSection(schema: FormSchema, sectionKey: string, direction: -1 | 1): FormSchema {
  const sorted = [...schema.sections].sort((a, b) => a.order - b.order);
  const idx = sorted.findIndex((s) => s.key === sectionKey);
  const targetIdx = idx + direction;
  if (idx < 0 || targetIdx < 0 || targetIdx >= sorted.length) return schema;
  [sorted[idx], sorted[targetIdx]] = [sorted[targetIdx], sorted[idx]];
  return { ...schema, sections: sorted.map((s, i) => ({ ...s, order: i })) };
}

export function updateSection(schema: FormSchema, sectionKey: string, patch: Partial<FormSection>): FormSchema {
  return mapSection(schema, sectionKey, (s) => ({ ...s, ...patch }));
}

export function addField(schema: FormSchema, sectionKey: string, key?: string): FormSchema {
  return mapSection(schema, sectionKey, (s) => ({
    ...s,
    fields: [...s.fields, emptyField(s.fields.length, key)],
  }));
}

export function removeField(schema: FormSchema, fieldKey: string): FormSchema {
  return {
    ...schema,
    sections: schema.sections.map((s) => ({ ...s, fields: s.fields.filter((f) => f.key !== fieldKey) })),
  };
}

export function moveField(schema: FormSchema, fieldKey: string, direction: -1 | 1): FormSchema {
  const section = findFieldSection(schema, fieldKey);
  if (!section) return schema;

  const sorted = [...section.fields].sort((a, b) => a.order - b.order);
  const idx = sorted.findIndex((f) => f.key === fieldKey);
  const targetIdx = idx + direction;
  if (idx < 0 || targetIdx < 0 || targetIdx >= sorted.length) return schema;
  [sorted[idx], sorted[targetIdx]] = [sorted[targetIdx], sorted[idx]];

  return mapSection(schema, section.key, (s) => ({ ...s, fields: sorted.map((f, i) => ({ ...f, order: i })) }));
}

export function updateField(schema: FormSchema, fieldKey: string, patch: Partial<FormField>): FormSchema {
  return mapField(schema, fieldKey, (f) => ({ ...f, ...patch }));
}

export function changeFieldType(schema: FormSchema, fieldKey: string, type: FormFieldType): FormSchema {
  return mapField(schema, fieldKey, (f) => ({
    ...f,
    type,
    options: ['Select', 'MultiSelect', 'Radio'].includes(type) ? f.options : [],
  }));
}

// ── Options ──

export function addOption(schema: FormSchema, fieldKey: string): FormSchema {
  return mapField(schema, fieldKey, (f) => {
    const option: FormFieldOption = { value: `option-${f.options.length + 1}`, label: 'New Option', order: f.options.length };
    return { ...f, options: [...f.options, option] };
  });
}

export function updateOption(schema: FormSchema, fieldKey: string, index: number, patch: Partial<FormFieldOption>): FormSchema {
  return mapField(schema, fieldKey, (f) => ({
    ...f,
    options: f.options.map((o, i) => (i === index ? { ...o, ...patch } : o)),
  }));
}

export function removeOption(schema: FormSchema, fieldKey: string, index: number): FormSchema {
  return mapField(schema, fieldKey, (f) => ({
    ...f,
    options: f.options.filter((_, i) => i !== index).map((o, i) => ({ ...o, order: i })),
  }));
}

export function moveOption(schema: FormSchema, fieldKey: string, index: number, direction: -1 | 1): FormSchema {
  return mapField(schema, fieldKey, (f) => {
    const targetIdx = index + direction;
    if (targetIdx < 0 || targetIdx >= f.options.length) return f;
    const options = [...f.options];
    [options[index], options[targetIdx]] = [options[targetIdx], options[index]];
    return { ...f, options: options.map((o, i) => ({ ...o, order: i })) };
  });
}

// ── Conditions ──

export function addCondition(schema: FormSchema, fieldKey: string): FormSchema {
  return mapField(schema, fieldKey, (f) => {
    const condition: FormFieldCondition = { sourceFieldKey: '', operator: 'Equals', value: '' };
    return {
      ...f,
      conditionCombinator: f.conditionCombinator ?? 'AND',
      conditions: [...f.conditions, condition],
    };
  });
}

export function updateCondition(schema: FormSchema, fieldKey: string, index: number, patch: Partial<FormFieldCondition>): FormSchema {
  return mapField(schema, fieldKey, (f) => ({
    ...f,
    conditions: f.conditions.map((c, i) => (i === index ? { ...c, ...patch } : c)),
  }));
}

export function removeCondition(schema: FormSchema, fieldKey: string, index: number): FormSchema {
  return mapField(schema, fieldKey, (f) => ({ ...f, conditions: f.conditions.filter((_, i) => i !== index) }));
}

export function allFieldsExcept(schema: FormSchema, fieldKey: string): FormField[] {
  return schema.sections.flatMap((s) => s.fields).filter((f) => f.key !== fieldKey);
}
