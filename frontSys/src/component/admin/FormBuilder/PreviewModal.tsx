import type { FormSchema } from '../../../api/types';
import { useAlert } from '../../../context/AlertContext';
import DynamicForm from '../../DynamicForm/DynamicForm';

interface PreviewModalProps {
  schema: FormSchema;
  onClose: () => void;
}

/**
 * Reuses the exact same DynamicForm the public site renders - conditional logic and validation
 * behave identically here, so the admin can genuinely test them, not just look at a mockup.
 */
const PreviewModal: React.FC<PreviewModalProps> = ({ schema, onClose }) => {
  const { info } = useAlert();

  return (
    <div className="admin-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="admin-modal"
        style={{ width: 'min(900px, 100%)' }}
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-label="Form preview"
      >
        <div className="admin-modal-header">
          <h3>Preview - {schema.formKey}</h3>
          <button type="button" className="admin-modal-close" onClick={onClose} aria-label="Close">
            <i className="ri-close-line" />
          </button>
        </div>
        <div className="admin-modal-body">
          <div className="admin-warn-box">
            This preview uses the unsaved draft and the real form engine, including conditional
            fields. Submitting here does not save anything.
          </div>
          <DynamicForm
            schema={schema}
            submitLabel="Submit (Preview)"
            onSubmit={() => {
              info('Preview only - nothing was submitted.');
            }}
          />
        </div>
      </div>
    </div>
  );
};

export default PreviewModal;
