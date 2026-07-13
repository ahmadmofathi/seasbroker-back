import { useState } from 'react';
import FormInput from '../Common/FormInput';
import { useNavigate } from 'react-router';
import { quoteApi } from '../../api';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import { withServiceTag } from '../../api/types';
import { toCargoTypeOptions } from '../../constants/cargoTypes';
import { getPortOptions } from '../../utils/portOptions';

const RequestClearance: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const { success, error: showError } = useAlert();

  const cargo_types = toCargoTypeOptions();
  const cities = getPortOptions().map((p) => ({ text: p.label, value: p.value }));

  const handleSubmit: React.FormEventHandler = (e) => {
    e.preventDefault();
    void submitClearance(e);
  };

  const submitClearance = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);

    const formData = new FormData(e.currentTarget as HTMLFormElement);
    const cargoType = String(formData.get('cargo_type') ?? '').trim();
    const weight = String(formData.get('weight') ?? '0');
    const departurePort = String(formData.get('departure_port') ?? '').trim();
    const deliveryPort = String(formData.get('destination_port') ?? '').trim();
    const containerType = String(formData.get('container_type') ?? '').trim();
    const firstName = String(formData.get('first_name') ?? '').trim();
    const lastName = String(formData.get('last_name') ?? '').trim();
    const email = String(formData.get('email') ?? '').trim();
    const phoneNumber = String(formData.get('phone_number') ?? '').trim();
    const message = String(formData.get('special_instructions') ?? '').trim();

    const now = new Date();
    const later = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000);

    try {
      const response = await quoteApi.submitQuote({
        cargoType: cargoType || 'Customs Clearance',
        weight: Number(weight) || 1,
        departurePort,
        departureTime: now.toISOString(),
        arrivalPort: deliveryPort,
        arrivalTime: later.toISOString(),
        dimensions: containerType || 'N/A',
        additionalInfo: withServiceTag(
          'Customs Clearance',
          `Container: ${containerType}. ${message}`.trim(),
        ),
        fname: firstName,
        lname: lastName,
        email,
        phoneNumber,
      });

      success(response.message || 'Clearance request submitted successfully.');
      void navigate(`/clearance_offices?delivery-port=${encodeURIComponent(deliveryPort)}`);
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section id="request_quote_form_area">
      <div className="container">
        <div className="row">
          <div className="col-lg-12 col-sm-12 col-md-12 col-12">
            <form id="request_form" onSubmit={handleSubmit}>
              <div className="row">
                <div className="col-lg-12">
                  <div className="heading_quote">
                    <h3>Request Customs Clearance Quote</h3>
                  </div>
                </div>

                <div className="col-lg-6">
                  <FormInput name="cargo_type" tag="select" classes="form-control" options={cargo_types} label="Cargo Type" />
                </div>
                <div className="col-lg-6">
                  <FormInput tag="input" type="number" name="weight" classes="form-control" placeholder="Total Weight (KG)" label="Total Weight (KG)" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="departure_port" tag="select" classes="form-control" options={cities} label="Departure Port" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="destination_port" tag="select" classes="form-control" options={cities} label="Destination Port" />
                </div>
                <div className="col-lg-6">
                  <FormInput
                    name="container_type"
                    tag="select"
                    classes="form-control"
                    options={[
                      { value: "20' Container", text: "20' Container" },
                      { value: "40' Container", text: "40' Container" },
                      { value: 'Refrigerated Container', text: 'Refrigerated Container' },
                      { value: 'Flat Rack', text: 'Flat Rack' },
                      { value: 'Open Top', text: 'Open Top' },
                    ]}
                    label="Container Type"
                  />
                </div>
                <div className="col-lg-12">
                  <div className="heading_quote area_top">
                    <h3>Your Contact Information</h3>
                  </div>
                </div>
                <div className="col-lg-6">
                  <FormInput tag="input" type="text" name="first_name" classes="form-control" placeholder="First Name" label="First Name" />
                </div>
                <div className="col-lg-6">
                  <FormInput tag="input" type="text" name="last_name" classes="form-control" placeholder="Last Name" label="Last Name" />
                </div>
                <div className="col-lg-6">
                  <FormInput tag="input" type="email" name="email" classes="form-control" placeholder="Email Address" label="Email Address" />
                </div>
                <div className="col-lg-6">
                  <FormInput tag="input" type="text" name="phone_number" classes="form-control" placeholder="Phone Number" label="Phone Number" />
                </div>
                <div className="col-lg-12">
                  <FormInput tag="textarea" type="text" name="special_instructions" classes="form-control" placeholder="Any Special Instructions" label="Special Instructions" />
                </div>
                <div className="col-lg-12">
                  <div className="quote_submit_button">
                    <button type="submit" className="btn btn-theme" disabled={submitting}>
                      {submitting ? 'Submitting…' : 'Submit and Proceed'}
                    </button>
                  </div>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    </section>
  );
};

export default RequestClearance;
