import React, { Fragment } from 'react';
import { Menu, Transition } from '@headlessui/react';
import { cn } from '../utils/cn';

interface DropdownMenuProps {
  trigger: React.ReactElement;
  children: React.ReactNode;
  align?: 'left' | 'right';
}

interface DropdownMenuItemProps {
  onClick?: () => void;
  children: React.ReactNode;
  className?: string;
  variant?: 'default' | 'danger';
}

export const DropdownMenu: React.FC<DropdownMenuProps> = ({ 
  trigger, 
  children, 
  align = 'right' 
}) => {
  const alignmentClasses = align === 'left' ? 'left-0' : 'right-0';

  return (
    <Menu as="div" className="relative inline-block text-left">
      <Menu.Button as={Fragment}>
        {trigger}
      </Menu.Button>

      <Transition
        as={Fragment}
        enter="transition ease-out duration-100"
        enterFrom="transform opacity-0 scale-95"
        enterTo="transform opacity-100 scale-100"
        leave="transition ease-in duration-75"
        leaveFrom="transform opacity-100 scale-100"
        leaveTo="transform opacity-0 scale-95"
      >
        <Menu.Items 
          className={cn(
            'absolute z-50 mt-2 w-56 origin-top-right divide-y divide-gray-100 rounded-lg bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none',
            alignmentClasses
          )}
        >
          <div className="py-1">
            {children}
          </div>
        </Menu.Items>
      </Transition>
    </Menu>
  );
};

export const DropdownMenuItem: React.FC<DropdownMenuItemProps> = ({ 
  onClick, 
  children, 
  className,
  variant = 'default' 
}) => {
  const variantClasses = {
    default: 'text-gray-700 hover:bg-gray-50 hover:text-gray-900',
    danger: 'text-red-700 hover:bg-red-50 hover:text-red-900',
  };

  return (
    <Menu.Item>
      {({ active }) => (
        <button
          onClick={onClick}
          className={cn(
            'group flex w-full items-center px-4 py-2 text-sm transition-colors',
            active && variantClasses[variant],
            !active && (variant === 'danger' ? 'text-red-600' : 'text-gray-700'),
            className
          )}
        >
          {children}
        </button>
      )}
    </Menu.Item>
  );
};

export const DropdownMenuSeparator: React.FC = () => (
  <div className="my-1 h-px bg-gray-200" />
);