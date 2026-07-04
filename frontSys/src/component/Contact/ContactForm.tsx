import { useState } from 'react';
import FormInput from '../Common/FormInput';
import { quoteApi } from '../../api';
import { formatApiError } from '../../utils/formatApiError';
import { withServiceTag } from '../../api/types';

const ContactForm: React.FC = () => {
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleSubmit: React.FormEventHandler = (e) => {
    e.preventDefault();
    void submitContact(e);
  };

  const submitContact = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setSubmitting(true);

    const form = e.currentTarget as HTMLFormElement;
    const formData = new FormData(form);
    const name = String(formData.get('name') ?? '').trim();
    const email = String(formData.get('email') ?? '').trim();
    const subject = String(formData.get('subject') ?? '').trim();
    const message = String(formData.get('message') ?? '').trim();

    const nameParts = name.split(/\s+/).filter(Boolean);
    const fname = nameParts[0] || name || 'Contact';
    const lname = nameParts.slice(1).join(' ') || 'Form';
    const now = new Date();
    const later = new Date(Date.now() + 24 * 60 * 60 * 1000);

    try {
      const response = await quoteApi.submitQuote({
        cargoType: 'Contact Inquiry',
        weight: 1,
        departurePort: 'N/A',
        departureTime: now.toISOString(),
        arrivalPort: 'N/A',
        arrivalTime: later.toISOString(),
        dimensions: 'N/A',
        additionalInfo: withServiceTag('Contact', `Subject: ${subject}. Message: ${message}`),
        fname,
        lname,
        email,
        phoneNumber: 'N/A',
      });

      setSuccess(response.message || 'Message sent successfully.');
      form.reset();
    } catch (err) {
      setError(formatApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="row">
        {error && (
          <div className="col-lg-12">
            <div className="alert alert-danger">{error}</div>
          </div>
        )}
        {success && (
          <div className="col-lg-12">
            <div className="alert alert-success">{success}</div>
          </div>
        )}
        <div className="col-lg-12">
          <FormInput tag="input" type="text" name="name" classes="form-control" placeholder="Name" />
        </div>
        <div className="col-lg-12">
          <FormInput tag="input" type="email" name="email" classes="form-control" placeholder="Email" />
        </div>
        <div className="col-lg-12">
          <FormInput tag="input" type="text" name="subject" classes="form-control" placeholder="Subject" />
        </div>
        <div className="col-lg-12">
          <FormInput tag="textarea" type="text" name="message" classes="form-control" placeholder="Type Your Messages..." />
        </div>
        <div className="col-lg-12">
          <div className="contact_form_submit">
            <button type="submit" className="btn btn-theme" disabled={submitting}>
              {submitting ? 'Sending…' : 'Send'}
            </button>
          </div>
        </div>
      </div>
    </form>
  );
};

export default ContactForm;
