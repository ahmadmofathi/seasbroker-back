import { useState } from 'react';
import ports from '../../utils/ports.json';
import FormInput from '../Common/FormInput';
import { quoteApi } from '../../api';
import { formatApiError } from '../../utils/formatApiError';
import { withServiceTag } from '../../api/types';

const RequestRouteForm: React.FC = () => {
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const vessel_types = [
    { value: 'Bulk Carrier', text: 'Bulk Carrier' },
    { value: 'Container Ship', text: 'Container Ship' },
    { value: 'Tanker', text: 'Tanker' },
    { value: 'Passenger Ship', text: 'Passenger Ship' },
    { value: 'Other', text: 'Other' },
  ];

  const cargo_types = [
    { value: 'General Cargo', text: 'General Cargo' },
    { value: 'Gas', text: 'Gas' },
    { value: 'Liquid', text: 'Liquid' },
    { value: 'Heavy', text: 'Heavy' },
    { value: 'Fragile', text: 'Fragile' },
    { value: 'Perishable', text: 'Perishable' },
    { value: 'Bulk', text: 'Bulk' },
    { value: 'Dry', text: 'Dry' },
    { value: 'Refrigerated', text: 'Refrigerated' },
    { value: 'Other', text: 'Other' },
  ];

  const cities = Object.values(ports).map((port) => ({
    text: `${port.name} - ${port.country}`,
    value: `${port.name} - ${port.country}`,
  }));

  const handleSubmit: React.FormEventHandler = (e) => {
    e.preventDefault();
    void submitRoute(e);
  };

  const submitRoute = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);

    const formData = new FormData(e.currentTarget as HTMLFormElement);
    const vesselName = String(formData.get('vessel_name') ?? '').trim();
    const vesselType = String(formData.get('vessel_type') ?? '').trim();
    const vesselCapacity = String(formData.get('vessel_capacity') ?? '').trim();
    const imo = String(formData.get('imo') ?? '').trim();
    const departureTime = String(formData.get('departure_time') ?? '');
    const arrivalTime = String(formData.get('arrival_time') ?? '');
    const departurePort = String(formData.get('departure_port') ?? '').trim();
    const arrivalPort = String(formData.get('arrival_port') ?? '').trim();
    const passingPorts = formData.getAll('passing_ports').map(String);
    const brokerName = String(formData.get('broker_name') ?? '').trim();
    const brokerEmail = String(formData.get('broker_email') ?? '').trim();
    const brokerPhone = String(formData.get('broker_phone_number') ?? '').trim();
    const cargoTypes = formData.getAll('cargo_type').map(String);
    const totalWeight = String(formData.get('total_cargo_weight') ?? '0');

    const nameParts = brokerName.split(/\s+/).filter(Boolean);
    const fname = nameParts[0] || brokerName || 'Broker';
    const lname = nameParts.slice(1).join(' ') || 'Ship Route';

    const notes = [
      `Vessel: ${vesselName}`,
      `IMO: ${imo}`,
      `Vessel type: ${vesselType}`,
      `Capacity (tons): ${vesselCapacity}`,
      `Transit ports: ${passingPorts.join(', ') || 'None'}`,
      `Cargo types: ${cargoTypes.join(', ') || 'N/A'}`,
      `Broker phone: ${brokerPhone}`,
    ].join(' | ');

    try {
      const response = await quoteApi.submitQuote({
        cargoType: cargoTypes[0] || vesselType || 'Ship Brokerage',
        weight: Number(totalWeight) || Number(vesselCapacity) || 1,
        departurePort,
        departureTime: departureTime ? new Date(departureTime).toISOString() : new Date().toISOString(),
        arrivalPort,
        arrivalTime: arrivalTime
          ? new Date(arrivalTime).toISOString()
          : new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
        dimensions: `Vessel capacity ${vesselCapacity || 'N/A'} tons`,
        additionalInfo: withServiceTag('Ship Brokerage', notes),
        fname,
        lname,
        email: brokerEmail,
        phoneNumber: brokerPhone,
      });

      alert(response.message || 'Ship route registered successfully. Our team will review it in the admin panel.');
      (e.currentTarget as HTMLFormElement).reset();
    } catch (err) {
      setError(formatApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section id="request_quote_form_area">
      <div className="container">
        <div className="row">
          <div className="col-lg-12">
            <form id="request_form" onSubmit={handleSubmit}>
              <div className="row">
                <div className="col-lg-12">
                  <div className="heading_quote">
                    <h3>Register Ship Brokerage Route</h3>
                  </div>
                </div>

                {error && (
                  <div className="col-lg-12">
                    <div className="alert alert-danger">{error}</div>
                  </div>
                )}

                <div className="col-lg-6">
                  <FormInput name="vessel_name" tag="input" type="text" classes="form-control" placeholder="Vessel Name" label="Vessel Name" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="vessel_type" tag="select" classes="form-control" options={vessel_types} label="Vessel Type" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="vessel_capacity" tag="input" type="number" classes="form-control" placeholder="Vessel Capacity" label="Vessel Capacity (Tons)" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="imo" tag="input" type="number" classes="form-control" placeholder="IMO Number" label="IMO Number" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="departure_time" tag="input" type="datetime-local" classes="form-control" label="Departure Time" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="arrival_time" tag="input" type="datetime-local" classes="form-control" label="Arrival Time" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="departure_port" tag="select" classes="form-control" options={cities} label="Departure Port" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="arrival_port" tag="select" classes="form-control" options={cities} label="Destination Port" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="passing_ports" tag="select" classes="form-control" options={cities} multiSelect label="Transit Ports" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="broker_name" tag="input" type="text" classes="form-control" placeholder="Broker Name" label="Broker Name" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="broker_email" tag="input" type="email" classes="form-control" placeholder="Broker Email" label="Broker Email" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="broker_phone_number" tag="input" type="tel" classes="form-control" placeholder="Broker Phone Number" label="Broker Phone Number" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="cargo_type" tag="select" classes="form-control" multiSelect options={cargo_types} label="Cargo Type" />
                </div>
                <div className="col-lg-6">
                  <FormInput name="total_cargo_weight" tag="input" type="number" classes="form-control" placeholder="Total Cargo Weight (KG)" label="Total Cargo Weight" />
                </div>

                <div className="col-lg-12">
                  <div className="quote_submit_button">
                    <button type="submit" className="btn btn-theme" disabled={submitting}>
                      {submitting ? 'Submitting…' : 'Submit'}
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

export default RequestRouteForm;
