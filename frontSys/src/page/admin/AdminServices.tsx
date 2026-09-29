import { useEffect, useState } from 'react';
import { adminCreate, adminList, adminUpdate } from '../../api/adminClient';
import { useAlert } from '../../context/AlertContext';
import { formatApiError } from '../../utils/formatApiError';
import { ServiceData } from '../../component/Common/Service/ServiceData';
import {
  DEFAULT_SERVICES_CONTENT,
  MAX_DETAILS_SECTIONS,
  SERVICES_SETTING_KEY,
  parseServicesContent,
  type ServiceCardText,
  type ServiceDetailsSection,
  type ServicesContent,
} from '../../content/servicesContent';

interface SettingRecord {
  id: string;
  key: string;
  value: string;
}

const sectionTitle: React.CSSProperties = { fontSize: '1rem', color: 'var(--admin-navy)', margin: '1.5rem 0 1rem' };

/**
 * Edits the text about our services shown on the public home and Services pages. Saved as one JSON
 * system setting; the images and the form each service links to stay fixed.
 */
const AdminServices: React.FC = () => {
  const [content, setContent] = useState<ServicesContent>(DEFAULT_SERVICES_CONTENT);
  const [settingId, setSettingId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const { success, error: showError } = useAlert();

  const load = () => {
    setLoading(true);
    adminList<SettingRecord>('settings', { page: 1, perPage: 100 })
      .then((items) => {
        const item = items.find((s) => s.key === SERVICES_SETTING_KEY);
        setSettingId(item?.id ?? null);
        setContent(parseServicesContent(item?.value));
      })
      .catch((e: unknown) => { showError(formatApiError(e)); })
      .finally(() => { setLoading(false); });
  };

  useEffect(() => {
    load();
  }, []);

  const setField = (key: keyof Omit<ServicesContent, 'services' | 'details'>, value: string) => {
    setContent((c) => ({ ...c, [key]: value }));
  };

  const setCard = (index: number, key: keyof ServiceCardText, value: string) => {
    setContent((c) => ({
      ...c,
      services: c.services.map((s, i) => (i === index ? { ...s, [key]: value } : s)),
    }));
  };

  const setDetailsTitle = (title: string) => {
    setContent((c) => ({ ...c, details: { ...c.details, title } }));
  };

  const setSection = (index: number, key: keyof ServiceDetailsSection, value: string) => {
    setContent((c) => ({
      ...c,
      details: {
        ...c.details,
        sections: c.details.sections.map((s, i) => (i === index ? { ...s, [key]: value } : s)),
      },
    }));
  };

  const addSection = () => {
    setContent((c) => ({
      ...c,
      details: { ...c.details, sections: [...c.details.sections, { heading: '', body: '' }] },
    }));
  };

  const removeSection = (index: number) => {
    setContent((c) => ({
      ...c,
      details: { ...c.details, sections: c.details.sections.filter((_, i) => i !== index) },
    }));
  };

  const save = async (e: React.FormEvent) => {
    e.preventDefault();
    const blank =
      [content.homeHeading, content.pageHeading].some((v) => !v.trim()) ||
      content.services.some((s) => !s.heading.trim() || !s.para.trim() || !s.button.trim());
    if (blank) {
      showError('Every heading, description and button text needs some text.');
      return;
    }
    if (!content.details.title.trim()) {
      showError('The Service Details page needs a title.');
      return;
    }
    if (content.details.sections.some((s) => !s.heading.trim() && !s.body.trim())) {
      showError('Remove the empty section on the Service Details page, or write something in it.');
      return;
    }
    if (content.details.sections.length === 0) {
      showError('The Service Details page needs at least one section.');
      return;
    }

    setSaving(true);
    try {
      const value = JSON.stringify(content);
      if (settingId) {
        await adminUpdate('settings', settingId, { value });
      } else {
        await adminCreate('settings', { key: SERVICES_SETTING_KEY, value });
      }
      success('Services text saved. It shows on the website straight away.');
      load();
    } catch (err: unknown) {
      showError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="admin-loading">
        <div className="admin-spinner" /> Loading services text…
      </div>
    );
  }

  return (
    <div className="admin-panel" style={{ padding: '1.5rem' }}>
      <form onSubmit={(e) => void save(e)}>
        <h2 style={{ fontSize: '1.2rem', color: 'var(--admin-navy)', margin: '0 0 0.5rem' }}>Services text on the website</h2>
        <p className="admin-result-text" style={{ margin: 0 }}>
          This is the text about our services on the home page and the Services page. Pictures and the form
          each button opens stay the same.
        </p>

        <h3 style={sectionTitle}>Home page — services section</h3>
        <div className="admin-form-grid">
          <div className="admin-field full">
            <label htmlFor="s-home-heading">Heading</label>
            <input id="s-home-heading" className="admin-input" value={content.homeHeading} onChange={(e) => { setField('homeHeading', e.target.value); }} />
          </div>
          <div className="admin-field full">
            <label htmlFor="s-home-para">Text under the heading</label>
            <textarea id="s-home-para" className="admin-input" rows={3} value={content.homePara} onChange={(e) => { setField('homePara', e.target.value); }} />
          </div>
        </div>

        <h3 style={sectionTitle}>Services page — top section</h3>
        <div className="admin-form-grid">
          <div className="admin-field full">
            <label htmlFor="s-page-heading">Heading</label>
            <input id="s-page-heading" className="admin-input" value={content.pageHeading} onChange={(e) => { setField('pageHeading', e.target.value); }} />
          </div>
          <div className="admin-field full">
            <label htmlFor="s-page-para">Text under the heading</label>
            <textarea id="s-page-para" className="admin-input" rows={3} value={content.pagePara} onChange={(e) => { setField('pagePara', e.target.value); }} />
          </div>
        </div>

        {content.services.map((card, index) => (
          <div key={ServiceData[index]?.link ?? index}>
            <h3 style={sectionTitle}>
              Service {index + 1}
              <span className="admin-result-text" style={{ marginInlineStart: '0.5rem', fontWeight: 400 }}>
                (button opens {ServiceData[index]?.link})
              </span>
            </h3>
            <div className="admin-form-grid">
              <div className="admin-field">
                <label htmlFor={`s-card-${String(index)}-heading`}>Title</label>
                <input id={`s-card-${String(index)}-heading`} className="admin-input" value={card.heading} onChange={(e) => { setCard(index, 'heading', e.target.value); }} />
              </div>
              <div className="admin-field">
                <label htmlFor={`s-card-${String(index)}-button`}>Button text</label>
                <input id={`s-card-${String(index)}-button`} className="admin-input" value={card.button} onChange={(e) => { setCard(index, 'button', e.target.value); }} />
              </div>
              <div className="admin-field full">
                <label htmlFor={`s-card-${String(index)}-para`}>Description</label>
                <textarea id={`s-card-${String(index)}-para`} className="admin-input" rows={3} value={card.para} onChange={(e) => { setCard(index, 'para', e.target.value); }} />
              </div>
            </div>
          </div>
        ))}

        <h2 style={{ fontSize: '1.2rem', color: 'var(--admin-navy)', margin: '2.5rem 0 0.5rem', borderTop: '1px solid var(--admin-border)', paddingTop: '1.5rem' }}>
          Service Details page
        </h2>
        <p className="admin-result-text" style={{ margin: 0 }}>
          The page at /service_details. The first two sections show the page's pictures. Leave an empty
          line between paragraphs. Contact details on this page come from System Settings.
        </p>
        <div className="admin-form-grid" style={{ marginTop: '1rem' }}>
          <div className="admin-field full">
            <label htmlFor="s-details-title">Page title</label>
            <input id="s-details-title" className="admin-input" value={content.details.title} onChange={(e) => { setDetailsTitle(e.target.value); }} />
          </div>
        </div>

        {content.details.sections.map((section, index) => (
          <div key={index}>
            <h3 style={{ ...sectionTitle, display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
              Section {index + 1}
              <button
                type="button"
                className="admin-btn-sm danger"
                onClick={() => { removeSection(index); }}
                disabled={content.details.sections.length <= 1}
                title={content.details.sections.length <= 1 ? 'The page needs at least one section' : undefined}
              >
                Remove
              </button>
            </h3>
            <div className="admin-form-grid">
              <div className="admin-field full">
                <label htmlFor={`s-sec-${String(index)}-heading`}>Heading</label>
                <input id={`s-sec-${String(index)}-heading`} className="admin-input" value={section.heading} onChange={(e) => { setSection(index, 'heading', e.target.value); }} />
              </div>
              <div className="admin-field full">
                <label htmlFor={`s-sec-${String(index)}-body`}>Text</label>
                <textarea id={`s-sec-${String(index)}-body`} className="admin-input" rows={6} value={section.body} onChange={(e) => { setSection(index, 'body', e.target.value); }} />
              </div>
            </div>
          </div>
        ))}
        <button
          type="button"
          className="admin-btn-sm outline"
          style={{ marginTop: '1rem' }}
          onClick={addSection}
          disabled={content.details.sections.length >= MAX_DETAILS_SECTIONS}
        >
          + Add section
        </button>

        <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1.5rem' }}>
          <button type="submit" className="admin-btn-sm primary" disabled={saving}>
            {saving ? 'Saving…' : 'Save'}
          </button>
          <button
            type="button"
            className="admin-btn-sm outline"
            disabled={saving}
            title="Fill the form with the original website text (not saved until you press Save)"
            onClick={() => { setContent(DEFAULT_SERVICES_CONTENT); }}
          >
            Restore original text
          </button>
        </div>
      </form>
    </div>
  );
};

export default AdminServices;
