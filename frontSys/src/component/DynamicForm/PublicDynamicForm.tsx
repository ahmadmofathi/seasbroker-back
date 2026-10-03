import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { getPublishedSchema, submitForm } from '../../api/forms';
import type { FormSchema } from '../../api/types';
import { useAlert } from '../../context/AlertContext';
import { formatApiError } from '../../utils/formatApiError';
import DynamicForm from './DynamicForm';

interface PublicDynamicFormProps {
  formKey: string;
  title: string;
  submitLabel?: string;
  successMessage?: string;
  redirectTo?: string;
}

/** Renders one of the 3 public request forms from its published schema, via the dynamic engine. */
const PublicDynamicForm: React.FC<PublicDynamicFormProps> = ({
  formKey,
  title,
  submitLabel = 'Submit Request',
  successMessage = 'Your request has been submitted. Our team will contact you shortly.',
  redirectTo = '/',
}) => {
  const navigate = useNavigate();
  const { success, error: showError } = useAlert();
  const [schema, setSchema] = useState<FormSchema | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  // Set once the request is registered: the customer keeps the number to follow it later.
  const [registered, setRegistered] = useState<{ trackingNumber: string; email: string } | null>(null);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    let cancelled = false;
    getPublishedSchema(formKey)
      .then((s) => {
        if (!cancelled) setSchema(s);
      })
      .catch((err: unknown) => {
        if (!cancelled) setLoadError(formatApiError(err));
      });
    return () => {
      cancelled = true;
    };
  }, [formKey]);

  if (loadError) {
    return (
      <div className="col-lg-12">
        <div className="alert alert-danger">{loadError}</div>
      </div>
    );
  }

  if (registered) {
    return (
      <div className="col-lg-12">
        <div className="heading_quote">
          <h3>Request received</h3>
        </div>
        <div className="alert alert-success" role="status">
          <p className="mb-2">{successMessage}</p>
          <p className="mb-1">Your tracking number is</p>
          <p className="d-flex align-items-center flex-wrap gap-2 mb-2">
            <strong style={{ fontSize: '1.6rem', letterSpacing: '0.05em' }}>{registered.trackingNumber}</strong>
            <button
              type="button"
              className="btn btn-sm btn-outline-success"
              onClick={() => {
                void navigator.clipboard.writeText(registered.trackingNumber).then(() => { setCopied(true); });
              }}
            >
              {copied ? 'Copied' : 'Copy'}
            </button>
          </p>
          <p className="mb-0">
            Keep this number. With the email address you registered with, it lets you follow your
            request any time from the Track Your Service page, and change your answers there until our team
            accepts it.
          </p>
        </div>
        <div className="d-flex flex-wrap gap-2">
          <button
            type="button"
            className="btn btn-theme"
            onClick={() => {
              void navigate('/your_shipment', {
                state: { number: registered.trackingNumber, email: registered.email },
              });
            }}
          >
            Track this request
          </button>
          <Link to={redirectTo} className="btn btn-outline-secondary">Back to home</Link>
        </div>
      </div>
    );
  }

  if (!schema) {
    return (
      <div className="col-lg-12 text-center py-5">
        <div className="spinner-border text-danger" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <DynamicForm
      schema={schema}
      formId="request_form"
      submitLabel={submitLabel}
      banner={
        <div className="row">
          <div className="col-lg-12">
            <div className="heading_quote">
              <h3>{title}</h3>
            </div>
          </div>
        </div>
      }
      onSubmit={async (values, files) => {
        try {
          const response = await submitForm(formKey, values, files);
          success(successMessage);
          if (response.trackingNumber) {
            const emailField = schema.sections
              .flatMap((s) => s.fields)
              .find((f) => f.systemFieldKey === 'Email');
            const email = emailField ? values[emailField.key] : undefined;
            setRegistered({
              trackingNumber: response.trackingNumber,
              email: typeof email === 'string' ? email : '',
            });
            window.scrollTo({ top: 0, behavior: 'smooth' });
          } else {
            void navigate(redirectTo);
          }
        } catch (err) {
          showError(formatApiError(err));
          throw err;
        }
      }}
    />
  );
};

export default PublicDynamicForm;
