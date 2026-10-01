import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import { useEffect, useState } from 'react';
import { Link, Navigate, useLocation } from 'react-router';
import SectionHeading from '../component/Common/SectionHeading';
import { trackRequest } from '../api/tracking';
import type { RequestTracking } from '../api/types';
import { SeasBrokerApiError } from '../api/client';
import { formatApiError } from '../utils/formatApiError';

/** What the tracking form (or the confirmation after registering) hands over via router state. */
export interface TrackLookup {
  number: string;
  email: string;
}

const TrackShipmentView: React.FC = () => {
  const location = useLocation();
  // The email is passed in router state, never in the URL.
  const lookup = location.state as TrackLookup | null;
  const [tracking, setTracking] = useState<RequestTracking | null>(null);
  const [error, setError] = useState<string | null>(null);

  const number = lookup?.number;
  const email = lookup?.email;

  useEffect(() => {
    if (!number || !email) return;
    let active = true;
    setTracking(null);
    setError(null);
    trackRequest(number, email)
      .then((result) => {
        if (active) setTracking(result);
      })
      .catch((err: unknown) => {
        if (!active) return;
        setError(
          err instanceof SeasBrokerApiError && err.status === 404
            ? "We couldn't find a request with that tracking number and email address."
            : formatApiError(err),
        );
      });
    return () => {
      active = false;
    };
  }, [number, email]);

  // Opened directly or refreshed: there's nothing to look up, so go back to the form.
  if (!number || !email) {
    const prefill = new URLSearchParams(location.search).get('id');
    return <Navigate to={prefill ? `/track_ship?id=${encodeURIComponent(prefill)}` : '/track_ship'} replace />;
  }

  return (
    <>
      <CommonBanner heading="Your Request" page="Your Request" />
      <section id="track_shipment_area">
        <div className="container">
          <SectionHeading heading="Your Request Status" />
          <div className="row">
            <div className="col-lg-8 offset-lg-2 col-md-12 col-sm-12 col-12">
              <div className="track_area_form">
                {error ? (
                  <>
                    <div className="alert alert-danger">{error}</div>
                    <p>Check the tracking number and use the same email address you registered with.</p>
                    <Link to="/track_ship" className="btn btn-theme">Try again</Link>
                  </>
                ) : !tracking ? (
                  <p>Loading your request…</p>
                ) : (
                  <>
                    <p className="mb-1 text-muted">{tracking.service}</p>
                    <h3 className="mb-3">{tracking.statusLabel}</h3>
                    <p>{tracking.statusDetail}</p>

                    <ol className="list-group list-group-numbered my-4">
                      {tracking.steps.map((step) => (
                        <li
                          key={step.key}
                          className={`list-group-item d-flex justify-content-between align-items-center${step.current ? ' fw-bold' : ''}${!step.done && !step.current ? ' text-muted' : ''}`}
                        >
                          <span className="ms-2 me-auto">{step.label}</span>
                          {step.done ? (
                            <span className="badge bg-success">Done</span>
                          ) : step.current ? (
                            <span className="badge bg-primary">Now</span>
                          ) : null}
                        </li>
                      ))}
                    </ol>

                    <p><strong>Tracking number:</strong> {tracking.trackingNumber}</p>
                    <p><strong>Registered on:</strong> {new Date(tracking.submittedAt).toLocaleDateString()}</p>
                    {tracking.type && <p><strong>Type:</strong> {tracking.type}</p>}
                    {(tracking.departurePort || tracking.arrivalPort) && (
                      <p><strong>Route:</strong> {tracking.departurePort ?? '—'} → {tracking.arrivalPort ?? '—'}</p>
                    )}
                    {tracking.cargoReference && <p><strong>Cargo listing:</strong> {tracking.cargoReference}</p>}
                    {tracking.vesselName && <p><strong>Vessel:</strong> {tracking.vesselName}</p>}

                    <Link to="/track_ship" className="btn btn-theme mt-2">Track another request</Link>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  );
};

export default TrackShipmentView;
