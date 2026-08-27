import PublicDynamicForm from '../DynamicForm/PublicDynamicForm';

const RequestQuoteForm: React.FC = () => (
  <PublicDynamicForm
    formKey="request-quote"
    title="Register Cargo"
    submitLabel="Register Cargo"
    successMessage="Your cargo request has been registered. Our team will contact you shortly."
  />
);

export default RequestQuoteForm;
