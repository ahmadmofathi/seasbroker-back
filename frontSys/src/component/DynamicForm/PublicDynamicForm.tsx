import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
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
          await submitForm(formKey, values, files);
          success(successMessage);
          void navigate(redirectTo);
        } catch (err) {
          showError(formatApiError(err));
          throw err;
        }
      }}
    />
  );
};

export default PublicDynamicForm;
