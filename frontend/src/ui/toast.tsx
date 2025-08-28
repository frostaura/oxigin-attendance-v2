import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { cva, type VariantProps } from 'class-variance-authority';
import { CheckCircleIcon, ExclamationTriangleIcon, InformationCircleIcon, XCircleIcon, XMarkIcon } from './icons';

// Toast variant styles
const toastVariants = cva(
  'relative flex w-full items-center justify-between space-x-4 overflow-hidden rounded-md border p-6 pr-8 shadow-lg transition-all data-[swipe=cancel]:translate-x-0 data-[swipe=end]:translate-x-[var(--radix-toast-swipe-end-x)] data-[swipe=move]:translate-x-[var(--radix-toast-swipe-move-x)] data-[swipe=move]:transition-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[swipe=end]:animate-out data-[state=closed]:fade-out-80 data-[state=closed]:slide-out-to-right-full data-[state=open]:slide-in-from-top-full data-[state=open]:sm:slide-in-from-bottom-full',
  {
    variants: {
      variant: {
        default: 'border-gray-200 bg-white text-gray-900',
        success: 'border-green-200 bg-green-50 text-green-900',
        warning: 'border-yellow-200 bg-yellow-50 text-yellow-900',
        error: 'border-red-200 bg-red-50 text-red-900',
        info: 'border-blue-200 bg-blue-50 text-blue-900',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  }
);

// Toast types
export interface ToastProps extends VariantProps<typeof toastVariants> {
  id: string;
  title?: string;
  description?: string;
  action?: ReactNode;
  duration?: number;
}

export interface Toast extends ToastProps {
  id: string;
}

// Toast context
interface ToastContextType {
  toasts: Toast[];
  addToast: (toast: Omit<Toast, 'id'>) => string;
  removeToast: (id: string) => void;
  dismissToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

// Toast provider
export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const addToast = (toast: Omit<Toast, 'id'>) => {
    const id = Math.random().toString(36).slice(2);
    const newToast = { ...toast, id };
    
    setToasts((prev) => [...prev, newToast]);

    // Auto remove toast after duration
    if (toast.duration !== 0) {
      setTimeout(() => {
        removeToast(id);
      }, toast.duration || 5000);
    }

    return id;
  };

  const removeToast = (id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  };

  const dismissToast = (id: string) => {
    removeToast(id);
  };

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast, dismissToast }}>
      {children}
      <ToastViewport />
    </ToastContext.Provider>
  );
}

// Toast viewport
function ToastViewport() {
  const { toasts, dismissToast } = useToast();

  return createPortal(
    <div className="fixed top-0 z-[100] flex max-h-screen w-full flex-col-reverse p-4 sm:bottom-0 sm:right-0 sm:top-auto sm:flex-col md:max-w-[420px]">
      {toasts.map((toast) => (
        <ToastComponent key={toast.id} toast={toast} onDismiss={() => dismissToast(toast.id)} />
      ))}
    </div>,
    document.body
  );
}

// Toast component
function ToastComponent({ toast, onDismiss }: { toast: Toast; onDismiss: () => void }) {
  const getIcon = () => {
    switch (toast.variant) {
      case 'success':
        return <CheckCircleIcon className="h-5 w-5 text-green-500" />;
      case 'warning':
        return <ExclamationTriangleIcon className="h-5 w-5 text-yellow-500" />;
      case 'error':
        return <XCircleIcon className="h-5 w-5 text-red-500" />;
      case 'info':
        return <InformationCircleIcon className="h-5 w-5 text-blue-500" />;
      default:
        return <InformationCircleIcon className="h-5 w-5 text-gray-500" />;
    }
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      onDismiss();
    }, toast.duration || 5000);

    return () => clearTimeout(timer);
  }, [toast.duration, onDismiss]);

  return (
    <div className={toastVariants({ variant: toast.variant })}>
      <div className="flex items-start space-x-3">
        {getIcon()}
        <div className="flex-1 space-y-1">
          {toast.title && (
            <div className="text-sm font-semibold leading-none tracking-tight">
              {toast.title}
            </div>
          )}
          {toast.description && (
            <div className="text-sm opacity-80">
              {toast.description}
            </div>
          )}
        </div>
      </div>
      {toast.action && (
        <div className="flex items-center justify-center">
          {toast.action}
        </div>
      )}
      <button
        onClick={onDismiss}
        className="absolute right-2 top-2 rounded-md p-1 text-foreground/50 opacity-0 transition-opacity hover:text-foreground focus:opacity-100 focus:outline-none focus:ring-2 group-hover:opacity-100"
      >
        <XMarkIcon className="h-4 w-4" />
      </button>
    </div>
  );
}

// Toast hook
export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
}

// Toast utility functions
export const toastUtils = {
  success: (addToast: (toast: Omit<Toast, 'id'>) => string, message: string, options?: Partial<Toast>) => {
    return addToast({
      variant: 'success',
      title: 'Success',
      description: message,
      ...options,
    });
  },
  error: (addToast: (toast: Omit<Toast, 'id'>) => string, message: string, options?: Partial<Toast>) => {
    return addToast({
      variant: 'error',
      title: 'Error',
      description: message,
      ...options,
    });
  },
  warning: (addToast: (toast: Omit<Toast, 'id'>) => string, message: string, options?: Partial<Toast>) => {
    return addToast({
      variant: 'warning',
      title: 'Warning',
      description: message,
      ...options,
    });
  },
  info: (addToast: (toast: Omit<Toast, 'id'>) => string, message: string, options?: Partial<Toast>) => {
    return addToast({
      variant: 'info',
      title: 'Info',
      description: message,
      ...options,
    });
  },
};