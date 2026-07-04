import { useState } from "react";
import FormSelect from "../Common/FormSelect";
import ports from "../../utils/ports.json";
import { quoteApi } from "../../api";
import { withServiceTag } from "../../api/types";
import { formatApiError } from "../../utils/formatApiError";
import { useNavigate } from "react-router";
import * as z from "zod";

interface CargoType {
  value: string;
  text: string;
}

const QuoteFormSchema = z.object({
  cargoType: z.string().trim().min(1, "Cargo type is required."),
  weight: z.number("Weight must be a number").gt(0, "Weight must be positive."),
  departurePort: z.string().trim().min(1, "Departure port is required."),
  departureTime: z.iso.datetime(),
  arrivalPort: z.string().trim().min(1, "Arrival port is required."),
  arrivalTime: z.iso.datetime(),
  dimensions: z.string().trim().min(1, "Dimensions are required.")
    .regex(/^\d+(\.[\d]+)?[\s]*x[\s]*\d+(\.[\d]+)?[\s]*x[\s]*[\d]+(\.[\d]+)?$/, "Invalid format. (Length x Width x Height)."),
  fname: z.string().trim().min(1, "First name is required."),
  lname: z.string().trim().min(1, "Last name is required."),
  email: z.email(),
  phoneNumber: z.string().trim().min(1, "Phone number is required."),
  additionalInfo: z.string(),
})

type RequestQuoteFormData = z.infer<typeof QuoteFormSchema>

const offsetInHours = Math.round((-new Date().getTimezoneOffset()) / 60);

const RequestQuoteForm: React.FC = () => {
  // Some of the options poperties are hardcoded according to the formfield property in FormSelect
  const [formData, setFormData] = useState<RequestQuoteFormData>({
    cargoType: '',
    weight: 0,
    departurePort: '',
    departureTime: '',
    arrivalPort: '',
    arrivalTime: '',
    dimensions: '',
    fname: '',
    lname: '',
    email: '',
    phoneNumber: '',
    additionalInfo: ''
  });

  const [errors, setErrors] = useState<z.core.$ZodFlattenedError<RequestQuoteFormData>>();
  const [submitError, setSubmitError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();

  const cargo_types: CargoType[] = [
    { value: "General Cargo", text: "General Cargo" },
    { value: "Gas", text: "Gas" },
    { value: "Liquid", text: "Liquid" },
    { value: "Heavy", text: "Heavy" },
    { value: "Fragile", text: "Fragile" },
    { value: "Perishable", text: "Perishable" },
    { value: "Bulk", text: "Bulk" },
    { value: "Dry", text: "Dry" },
    { value: "Refrigerated", text: "Refrigerated" },
    { value: "Other", text: "Other" },
  ];

  // Create a cities array using port data
  const cities = [
    ...Object.values(ports).map(port => ({
      text: `${port.name} - ${port.country}`,
      value: `${port.name} - ${port.country}`,
      // FIXME: There are missing values for timezone in the ports.json
      // timezone: port.timezone? port.timezone : "UTC"
    }))
  ];


  const handleSubmit: React.FormEventHandler = (e) => {
    e.preventDefault();

    const toRFC3339 = (dt: string) => {
      return new Date(dt).toISOString();
    };

    const data = {
      ...formData,
      departureTime: formData.departureTime !== "" ? toRFC3339(formData.departureTime) : "",
      arrivalTime: formData.arrivalTime !== "" ? toRFC3339(formData.arrivalTime) : "",
    };

    const result = QuoteFormSchema.safeParse(data);

    if (!result.success) {
      // setErrors(z.flattenError(result.error));
      setErrors(z.flattenError(result.error));
      return;
    }
    setErrors(undefined);
    setSubmitError('');
    setSubmitting(true);

    quoteApi
      .submitQuote({
        ...result.data,
        additionalInfo: withServiceTag('Cargo Brokerage', result.data.additionalInfo),
      })
      .then((response) => {
        setSubmitting(false);
        alert(
          (response.message || 'Quote request submitted successfully') +
            '\n\nIt is now available in Admin → Public Requests.',
        );
        void navigate('/');
      })
      .catch((error: unknown) => {
        setSubmitting(false);
        setSubmitError(formatApiError(error));
      });
  };

  return (
    <form id="request_form" onSubmit={handleSubmit}>
      <div className="row">
        <div className="col-lg-12">
          <div className="heading_quote">
            <h3>Get a quote</h3>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="cargoType" className="form-label">Cargo Type</label>
            <select
              name="cargoType"
              className={`form-control${errors?.fieldErrors.cargoType ? ' is-invalid' : ''}`}

              onChange={(e) => { setFormData({ ...formData, cargoType: e.target.value }) }}
            >
              <option value="">Select Cargo Type</option>
              {cargo_types.map((type) => (
                <option key={type.value} value={type.value}>
                  {type.text}
                </option>
              ))}
            </select>
            {errors?.fieldErrors.cargoType && <div className="invalid-feedback">{errors.fieldErrors.cargoType.join(" ")}</div>}
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="weight" className="form-label">Total gross weight (KG)</label>
            <input
              type="number"
              name="weight"
              step="any"
              inputMode="decimal"
              className={`form-control${errors?.fieldErrors.weight ? ' is-invalid' : ''}`}
              placeholder="Total gross weight (KG)"
              onChange={(e) => {
                setFormData(prev => (
                  {
                    ...prev,
                    weight: e.target.value == "" ? 0 : parseFloat(e.target.value)
                  }))
              }}
            />
            {errors?.fieldErrors.weight && <div className="invalid-feedback">{errors.fieldErrors.weight}</div>}
          </div>

        </div>

        <div className="col-lg-6">
          <FormSelect<RequestQuoteFormData>
            label="Departure Port"
            placeholder="Select Departure Port"
            options={cities}
            formField='departurePort'
            error={errors?.fieldErrors.departurePort ? errors.fieldErrors.departurePort.join(" ") : ""}
            formData={formData}
            setFormData={setFormData}
          />
        </div>

        <div className="col-lg-6">
          <FormSelect<RequestQuoteFormData>
            label="Arrival Port"
            placeholder="Select Arrival Port"
            options={cities}
            formField='arrivalPort'
            error={errors?.fieldErrors.arrivalPort ? errors.fieldErrors.arrivalPort.join(" ") : ""}
            formData={formData}
            setFormData={setFormData}
          />
        </div>

        <p id="timeHelpBlock" className="form-text">
          Departure and arrival time use your current local time (and not the ports' local time)
        </p>
        {/*departure time and arrival time */}
        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="departureTime" className="form-label">Departure Time</label>
            {` (${offsetInHours >= 0 ? "+" : ""}${offsetInHours.toString()} UTC)`}
            <input
              type="datetime-local"
              name="departureTime"
              className={`form-control${errors?.fieldErrors.departureTime ? ' is-invalid' : ''}`}
              onChange={(e) => { setFormData({ ...formData, departureTime: e.target.value }) }}
              aria-describedby="timeHelpBlock"
            />
            {errors?.fieldErrors.departureTime && <div className="invalid-feedback">{errors.fieldErrors.departureTime.join(" ")}</div>}

          </div>
        </div>
        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="arrivalTime" className="form-label">Arrival Time</label>
            {` (${offsetInHours >= 0 ? "+" : ""}${offsetInHours.toString()} UTC)`}
            <input
              type="datetime-local"
              name="arrivalTime"
              className={`form-control${errors?.fieldErrors.arrivalTime ? ' is-invalid' : ''}`}
              onChange={(e) => { setFormData({ ...formData, arrivalTime: e.target.value }) }}
              aria-describedby="timeHelpBlock"
            />
            {errors?.fieldErrors.arrivalTime && <div className="invalid-feedback">{errors.fieldErrors.arrivalTime.join(" ")}</div>}

          </div>
        </div>

        {/* NOTE: Consider using 3 different inputs for each dimension */}
        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="dimensions" className="form-label">Dimensions (M)</label>
            <input
              type="text"
              name="dimensions"
              className={`form-control${errors?.fieldErrors.dimensions ? ' is-invalid' : ''}`}
              placeholder="Length x Width x Height (M) eg. 50 x 100 x 20.2"
              onChange={(e) => { setFormData({ ...formData, dimensions: e.target.value }) }}
            />
            {errors?.fieldErrors.dimensions && <div className="invalid-feedback">{errors.fieldErrors.dimensions.join(" ")}</div>}
          </div>
        </div>

        {/* Personal details section */}
        <div className="col-lg-12">
          <div className="heading_quote arae_top">
            <h3>Your Personal Details</h3>
          </div>
        </div>

        {/* Name */}
        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="fname" className="form-label">First Name</label>
            <input
              type="text"
              name="fname"
              className={`form-control${errors?.fieldErrors.fname ? ' is-invalid' : ''}`}
              placeholder="First Name"
              onChange={(e) => { setFormData({ ...formData, fname: e.target.value }) }}
            />
            {errors?.fieldErrors.fname && <div className="invalid-feedback">{errors.fieldErrors.fname.join(" ")}</div>}
          </div>
        </div>
        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="lname" className="form-label">Last Name</label>
            <input
              type="text"
              name="lname"
              className={`form-control${errors?.fieldErrors.lname ? ' is-invalid' : ''}`}
              placeholder="Last Name"
              onChange={(e) => { setFormData({ ...formData, lname: e.target.value }) }}
            />
            {errors?.fieldErrors.lname && <div className="invalid-feedback">{errors.fieldErrors.lname.join(" ")}</div>}

          </div>
        </div>

        {/* Contact Information */}
        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="email" className="form-label">Your Email</label>
            <input
              type="email"
              name="email"
              className={`form-control${errors?.fieldErrors.email ? ' is-invalid' : ''}`}
              placeholder="Email"
              onChange={(e) => { setFormData({ ...formData, email: e.target.value }) }}
            />
            {errors?.fieldErrors.email && <div className="invalid-feedback">{errors.fieldErrors.email.join(" ")}</div>}
          </div>
        </div>
        <div className="col-lg-6">
          <div className="form-group">
            <label htmlFor="phoneNumber" className="form-label">Your Phone Number</label>
            <input
              type="tel"
              name="phoneNumber"
              className={`form-control${errors?.fieldErrors.phoneNumber ? ' is-invalid' : ''}`}
              placeholder="Phone Number"
              onChange={(e) => { setFormData({ ...formData, phoneNumber: e.target.value }) }}
            />
            {errors?.fieldErrors.phoneNumber && <div className="invalid-feedback">{errors.fieldErrors.phoneNumber.join(" ")}</div>}
          </div>
        </div>

        <div className="col-lg-12">
          <div className="quote_submit_button">
            <button type="submit" className="btn btn-theme" disabled={submitting}>
              {submitting ? 'Submitting…' : 'Request Quote'}
            </button>
            {errors !== undefined && (
              <div className="alert alert-danger mt-3">
                Please fix the errors above before submitting.
              </div>
            )}
            {submitError && (
              <div className="alert alert-danger mt-3">{submitError}</div>
            )}
          </div>
        </div>
      </div>
    </form>
  )
}

export default RequestQuoteForm;