import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react';

export type AlertType = 'success' | 'error' | 'warning' | 'info';

interface ToastItem {
  id: string;
  type: AlertType;
  title?: string;
  message: string;
}

export interface ToastOptions {
  type?: AlertType;
  title?: string;
  message: string;
  duration?: number;
}

export interface ConfirmOptions {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  variant?: 'primary' | 'danger';
}

interface AlertContextValue {
  toast: (options: ToastOptions) => void;
  success: (message: string, title?: string) => void;
  error: (message: string, title?: string) => void;
  warning: (message: string, title?: string) => void;
  info: (message: string, title?: string) => void;
  confirm: (options: ConfirmOptions) => Promise<boolean>;
}

const AlertContext = createContext<AlertContextValue | null>(null);

const TOAST_ICONS: Record<AlertType, string> = {
  success: 'ri-checkbox-circle-fill',
  error: 'ri-error-warning-fill',
  warning: 'ri-alert-fill',
  info: 'ri-information-fill',
};

const DEFAULT_TITLES: Record<AlertType, string> = {
  success: 'Success',
  error: 'Error',
  warning: 'Warning',
  info: 'Notice',
};

const DEFAULT_DURATION: Record<AlertType, number> = {
  success: 5000,
  error: 8000,
  warning: 6000,
  info: 5000,
};

let toastCounter = 0;

export function AlertProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const [confirmState, setConfirmState] = useState<
    (ConfirmOptions & { resolve: (value: boolean) => void }) | null
  >(null);
  const timersRef = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map());

  const removeToast = useCallback((id: string) => {
    const timer = timersRef.current.get(id);
    if (timer) {
      clearTimeout(timer);
      timersRef.current.delete(id);
    }
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const toast = useCallback(
    ({ type = 'info', title, message, duration }: ToastOptions) => {
      const id = `toast-${++toastCounter}`;
      setToasts((prev) => [...prev, { id, type, title, message }]);
      const ms = duration ?? DEFAULT_DURATION[type];
      const timer = setTimeout(() => removeToast(id), ms);
      timersRef.current.set(id, timer);
    },
    [removeToast],
  );

  const success = useCallback(
    (message: string, title?: string) => toast({ type: 'success', message, title }),
    [toast],
  );
  const error = useCallback(
    (message: string, title?: string) => toast({ type: 'error', message, title }),
    [toast],
  );
  const warning = useCallback(
    (message: string, title?: string) => toast({ type: 'warning', message, title }),
    [toast],
  );
  const info = useCallback(
    (message: string, title?: string) => toast({ type: 'info', message, title }),
    [toast],
  );

  const confirm = useCallback((options: ConfirmOptions) => {
    return new Promise<boolean>((resolve) => {
      setConfirmState({ ...options, resolve });
    });
  }, []);

  const closeConfirm = (result: boolean) => {
    confirmState?.resolve(result);
    setConfirmState(null);
  };

  const value = useMemo(
    () => ({ toast, success, error, warning, info, confirm }),
    [toast, success, error, warning, info, confirm],
  );

  return (
    <AlertContext.Provider value={value}>
      {children}

      <div className="sb-toast-stack" aria-live="polite" aria-relevant="additions">
        {toasts.map((t) => (
          <div key={t.id} className={`sb-toast ${t.type}`} role="alert">
            <div className="sb-toast-icon">
              <i className={TOAST_ICONS[t.type]} aria-hidden />
            </div>
            <div className="sb-toast-body">
              <div className="sb-toast-title">{t.title ?? DEFAULT_TITLES[t.type]}</div>
              <div className="sb-toast-message">{t.message}</div>
            </div>
            <button
              type="button"
              className="sb-toast-close"
              aria-label="Dismiss"
              onClick={() => removeToast(t.id)}
            >
              <i className="ri-close-line" />
            </button>
          </div>
        ))}
      </div>

      {confirmState && (
        <div
          className="sb-confirm-overlay"
          role="presentation"
          onClick={() => closeConfirm(false)}
        >
          <div
            className="sb-confirm-dialog"
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="sb-confirm-title"
            onClick={(e) => e.stopPropagation()}
          >
            <div className={`sb-confirm-header ${confirmState.variant === 'danger' ? 'danger' : ''}`}>
              <i className={confirmState.variant === 'danger' ? 'ri-delete-bin-line' : 'ri-question-line'} />
              <h3 id="sb-confirm-title">{confirmState.title ?? 'Confirm'}</h3>
            </div>
            <div className="sb-confirm-body">{confirmState.message}</div>
            <div className="sb-confirm-footer">
              <button
                type="button"
                className="sb-confirm-btn cancel"
                onClick={() => closeConfirm(false)}
              >
                {confirmState.cancelText ?? 'Cancel'}
              </button>
              <button
                type="button"
                className={`sb-confirm-btn confirm${confirmState.variant === 'danger' ? ' danger' : ''}`}
                onClick={() => closeConfirm(true)}
              >
                {confirmState.confirmText ?? 'Confirm'}
              </button>
            </div>
          </div>
        </div>
      )}
    </AlertContext.Provider>
  );
}

export function useAlert(): AlertContextValue {
  const ctx = useContext(AlertContext);
  if (!ctx) {
    throw new Error('useAlert must be used within AlertProvider');
  }
  return ctx;
}
