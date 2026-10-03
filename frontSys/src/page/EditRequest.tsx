import { useEffect, useMemo, useState } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router';
import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import DynamicForm from '../component/DynamicForm/DynamicForm';
import type { FieldValue } from '../component/DynamicForm/conditionEngine';
import { loadRequestForEdit, saveRequestEdit } from '../api/tracking';
import type { RequestEditForm } from '../api/types';
import { SeasBrokerApiError } from '../api/client';
import { useAlert } from '../context/AlertContext';
import { formatApiError } from '../utils/formatApiError';
import type { TrackLookup } from './TrackShipmentView';

/**
 * Lets a customer change the answers on a request they already sent, until the team accepts it.
 * Reached from the tracking page, which hands over the tracking number and email in router state.
 */
const EditRequest: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { success, error: showError } = useAlert();
  const lookup = location.state as TrackLookup | null;
  const [form, setForm] = useState<RequestEditForm | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);

  const number = lookup?.number;
  const email = lookup?.email;

  useEffect(() => {
    if (!number || !email) return;
    let active = true;
    loadRequestForEdit(number, email)
      .then((loaded) => {
        if (active) setForm(loaded);
      })
      .catch((err: unknown) => {
        if (active) setLoadError(formatApiError(err));
      });
    return () => {
      active = false;
    };
  }, [number, email]);

  // The email identifies the customer, so it's shown but can't be changed.
  const emailKey = useMemo(
    () => form?.schema.sections.flatMap((s) => s.fields).find((f) => f.systemFieldKey === 'Email')?.key,
    [form],
  );

  if (!number || !email) {
    return <Navigate to="/track_ship" replace />;
  }

  const back = () => {
    void navigate('/your_shipment', { state: { number, email } satisfies TrackLookup });
  };

  return (
    <>
      <CommonBanner heading="Edit Your Request" page="Edit Your Request" />
      <section id="request_quote_form_area">
        <div className="container">
          <div className="row">
            <div className="col-lg-12 col-sm-12 col-md-12 col-12">
              {loadError ? (
                <>
                  <div className="alert alert-warning">{loadError}</div>
                  <button type="button" className="btn btn-theme" onClick={back}>Back to my request</button>
                </>
              ) : !form ? (
                <div className="text-center py-5">
                  <div className="spinner-border text-danger" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </div>
              ) : (
                <>
                  <div className="heading_quote">
                    <h3>Edit your request</h3>
                  </div>
                  <p>
                    Tracking number <strong>{form.trackingNumber}</strong>. You can change your answers until our
                    team accepts your request. Your email address can't be changed.
                  </p>
                  {form.files.length > 0 && (
                    <p className="text-muted">
                      Attached files ({form.files.map((f) => f.fileName).join(', ')}) stay as they are. To change
                      them, please contact us.
                    </p>
                  )}
                  <DynamicForm
                    schema={form.schema}
                    formId="edit_request_form"
                    submitLabel="Save changes"
                    initialValues={form.values as Record<string, FieldValue>}
                    lockedKeys={emailKey ? [emailKey] : undefined}
                    hideFileFields
                    onSubmit={async (values) => {
                      try {
                        await saveRequestEdit(number, email, values);
                        success('Your changes have been saved.');
                        back();
                      } catch (err) {
                        // Accepted in the meantime: say so, rather than showing a form that can't be saved.
                        if (err instanceof SeasBrokerApiError && err.status === 409) {
                          setLoadError(formatApiError(err));
                        } else {
                          showError(formatApiError(err));
                        }
                        throw err;
                      }
                    }}
                    banner={undefined}
                  />
                  <p className="mt-3">
                    <Link to="/your_shipment" state={{ number, email } satisfies TrackLookup}>Cancel and go back</Link>
                  </p>
                </>
              )}
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  );
};

export default EditRequest;
