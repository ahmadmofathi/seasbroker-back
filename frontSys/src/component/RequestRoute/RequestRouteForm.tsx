import PublicDynamicForm from '../DynamicForm/PublicDynamicForm';

const RequestRouteForm: React.FC = () => (
  <PublicDynamicForm
    formKey="request-route"
    title="Register Ship Brokerage"
    submitLabel="Submit Vessel Details"
    successMessage="Your vessel details have been submitted. Our team will contact you shortly."
  />
);

export default RequestRouteForm;
