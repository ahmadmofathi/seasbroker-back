import React, { useEffect, useState } from 'react';
import { listCollection, updateRecord } from '../../api/client';
import { useAlert } from '../../context/AlertContext';
import { formatApiError } from '../../utils/formatApiError';

interface SettingRecord {
  id: string;
  key: string;
  value: string;
}

const AdminSettings: React.FC = () => {
  const [settings, setSettings] = useState<SettingRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  
  // Local state for each setting field
  const [address, setAddress] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [facebook, setFacebook] = useState('');
  const [twitter, setTwitter] = useState('');
  const [linkedin, setLinkedin] = useState('');
  const [instagram, setInstagram] = useState('');
  const [privacyPolicy, setPrivacyPolicy] = useState('');
  const [termsConditions, setTermsConditions] = useState('');

  const { success, error: showError } = useAlert();

  const loadSettings = () => {
    setLoading(true);
    listCollection<SettingRecord>('settings', { page: 1, perPage: 100 })
      .then((res) => {
        const items = res.items || [];
        setSettings(items);
        
        // Map fetched settings to form states
        items.forEach((item) => {
          switch (item.key) {
            case 'address': setAddress(item.value); break;
            case 'phone': setPhone(item.value); break;
            case 'email': setEmail(item.value); break;
            case 'facebook': setFacebook(item.value); break;
            case 'twitter': setTwitter(item.value); break;
            case 'linkedin': setLinkedin(item.value); break;
            case 'instagram': setInstagram(item.value); break;
            case 'privacy_policy': setPrivacyPolicy(item.value); break;
            case 'terms_conditions': setTermsConditions(item.value); break;
          }
        });
      })
      .catch((e: unknown) => showError(formatApiError(e)))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadSettings();
  }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      const updates = [
        { key: 'address', value: address },
        { key: 'phone', value: phone },
        { key: 'email', value: email },
        { key: 'facebook', value: facebook },
        { key: 'twitter', value: twitter },
        { key: 'linkedin', value: linkedin },
        { key: 'instagram', value: instagram },
        { key: 'privacy_policy', value: privacyPolicy },
        { key: 'terms_conditions', value: termsConditions },
      ];

      for (const update of updates) {
        const matched = settings.find((s) => s.key === update.key);
        if (matched) {
          // Only update if value changed to save requests
          if (matched.value !== update.value) {
            await updateRecord('settings', matched.id, { value: update.value });
          }
        }
      }

      success('System settings saved successfully.');
      loadSettings(); // Reload to refresh stored state values
    } catch (err: unknown) {
      showError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="admin-panel" style={{ padding: '2rem', textAlign: 'center' }}>
        <p style={{ color: 'var(--admin-muted)' }}>Loading system settings...</p>
      </div>
    );
  }

  return (
    <div className="admin-panel" style={{ background: '#fff', borderRadius: 'var(--admin-radius)', padding: '1.5rem', boxShadow: 'var(--admin-shadow)' }}>
      <form onSubmit={handleSave}>
        <h2 style={{ fontSize: '1.2rem', color: 'var(--admin-navy)', margin: '0 0 1.5rem', borderBottom: '1px solid var(--admin-border)', paddingBottom: '0.5rem' }}>
          Update Seasbroker Info & Settings
        </h2>

        {/* Contact Information Section */}
        <h3 style={{ fontSize: '1rem', color: 'var(--admin-navy)', margin: '1.5rem 0 1rem' }}>Contact Information</h3>
        <div className="admin-form-grid" style={{ marginBottom: '2rem' }}>
          <div className="admin-field">
            <label>Address</label>
            <input
              type="text"
              className="admin-input"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              placeholder="e.g. Alexandria, Egypt"
              required
            />
          </div>
          <div className="admin-field">
            <label>Phone Number</label>
            <input
              type="text"
              className="admin-input"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              placeholder="e.g. +20 102 3456 789"
              required
            />
          </div>
          <div className="admin-field full">
            <label>Contact Email</label>
            <input
              type="email"
              className="admin-input"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="e.g. info@seasbroker.com"
              required
            />
          </div>
        </div>

        {/* Social Media Links Section */}
        <h3 style={{ fontSize: '1rem', color: 'var(--admin-navy)', margin: '1.5rem 0 1rem' }}>Social Media Links</h3>
        <div className="admin-form-grid" style={{ marginBottom: '2rem' }}>
          <div className="admin-field">
            <label>Facebook URL</label>
            <input
              type="text"
              className="admin-input"
              value={facebook}
              onChange={(e) => setFacebook(e.target.value)}
              placeholder="https://facebook.com/..."
            />
          </div>
          <div className="admin-field">
            <label>Twitter / X URL</label>
            <input
              type="text"
              className="admin-input"
              value={twitter}
              onChange={(e) => setTwitter(e.target.value)}
              placeholder="https://twitter.com/..."
            />
          </div>
          <div className="admin-field">
            <label>LinkedIn URL</label>
            <input
              type="text"
              className="admin-input"
              value={linkedin}
              onChange={(e) => setLinkedin(e.target.value)}
              placeholder="https://linkedin.com/in/..."
            />
          </div>
          <div className="admin-field">
            <label>Instagram URL</label>
            <input
              type="text"
              className="admin-input"
              value={instagram}
              onChange={(e) => setInstagram(e.target.value)}
              placeholder="https://instagram.com/..."
            />
          </div>
        </div>

        {/* Policies Section */}
        <h3 style={{ fontSize: '1rem', color: 'var(--admin-navy)', margin: '1.5rem 0 0.5rem' }}>Policies &amp; Agreements</h3>
        <p style={{ fontSize: '0.8rem', color: 'var(--admin-muted)', marginBottom: '1rem' }}>
          You can enter plain text or HTML. Content is shown directly on the Privacy Policy and Terms &amp; Conditions public pages.
        </p>
        <div style={{ marginBottom: '1.5rem' }}>
          <div className="admin-field" style={{ marginBottom: '1.25rem' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <i className="ri-shield-check-line" style={{ color: 'var(--admin-red)' }} />
              Privacy Policy
            </label>
            <textarea
              className="admin-input"
              rows={6}
              value={privacyPolicy}
              onChange={(e) => setPrivacyPolicy(e.target.value)}
              placeholder="Enter Privacy Policy content here..."
              style={{ resize: 'vertical', minHeight: '120px' }}
            />
          </div>
          <div className="admin-field">
            <label style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <i className="ri-file-text-line" style={{ color: 'var(--admin-red)' }} />
              Terms &amp; Conditions
            </label>
            <textarea
              className="admin-input"
              rows={6}
              value={termsConditions}
              onChange={(e) => setTermsConditions(e.target.value)}
              placeholder="Enter Terms & Conditions content here..."
              style={{ resize: 'vertical', minHeight: '120px' }}
            />
          </div>
        </div>

        {/* Action Button */}
        <div style={{ marginTop: '2rem', display: 'flex', gap: '1rem' }}>
          <button type="submit" className="admin-btn-sm primary" disabled={saving}>
            <i className="ri-save-line" /> {saving ? 'Saving...' : 'Save Settings'}
          </button>
          <button type="button" className="admin-btn-sm outline" onClick={loadSettings} disabled={saving}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

export default AdminSettings;
