import React, { Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { cn } from '../utils/cn';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  children: React.ReactNode;
  className?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl' | '2xl';
}

const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  children,
  className,
  size = 'md'
}) => {
  const sizeClasses = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-xl',
    '2xl': 'max-w-2xl',
  };

  return (
    <Transition appear show={isOpen} as={Fragment}>
      <Dialog as="div" className="relative z-50" onClose={onClose}>
        <Transition.Child
          as={Fragment}
          enter="ease-out duration-300"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="ease-in duration-200"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-black bg-opacity-25 backdrop-blur-sm" />
        </Transition.Child>

        <div className="fixed inset-0 overflow-y-auto">
          <div className="flex min-h-full items-center justify-center p-4 text-center">
            <Transition.Child
              as={Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <Dialog.Panel
                className={cn(
                  'w-full transform overflow-hidden rounded-2xl bg-white p-6 text-left align-middle shadow-xl transition-all',
                  sizeClasses[size],
                  className
                )}
              >
                {children}
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </div>
      </Dialog>
    </Transition>
  );
};

interface ModalHeaderProps {
  children: React.ReactNode;
  className?: string;
}

const ModalHeader: React.FC<ModalHeaderProps> = ({ children, className }) => (
  <Dialog.Title
    as="h3"
    className={cn("text-lg font-semibold leading-6 text-gray-900 mb-4", className)}
  >
    {children}
  </Dialog.Title>
);

interface ModalBodyProps {
  children: React.ReactNode;
  className?: string;
}

const ModalBody: React.FC<ModalBodyProps> = ({ children, className }) => (
  <div className={cn("text-sm text-gray-600 mb-6", className)}>
    {children}
  </div>
);

interface ModalFooterProps {
  children: React.ReactNode;
  className?: string;
}

const ModalFooter: React.FC<ModalFooterProps> = ({ children, className }) => (
  <div className={cn("flex gap-3 justify-end", className)}>
    {children}
  </div>
);

export { Modal, ModalHeader, ModalBody, ModalFooter };