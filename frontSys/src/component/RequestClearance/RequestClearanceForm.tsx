import PublicDynamicForm from '../DynamicForm/PublicDynamicForm';

const RequestClearanceForm: React.FC = () => (
  <PublicDynamicForm
    formKey="request-clearance"
    title="Request Customs Clearance"
    submitLabel="Submit Clearance Request"
    successMessage="Your customs clearance request has been submitted. Our team will contact you shortly."
  />
);

export default RequestClearanceForm;
