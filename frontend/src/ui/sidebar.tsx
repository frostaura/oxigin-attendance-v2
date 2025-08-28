import React, { Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { cn } from '../utils/cn';

interface SidebarProps {
  isOpen?: boolean;
  onClose?: () => void;
  children: React.ReactNode;
  className?: string;
  variant?: 'mobile' | 'desktop';
}

interface SidebarHeaderProps {
  children: React.ReactNode;
  className?: string;
}

interface SidebarContentProps {
  children: React.ReactNode;
  className?: string;
}

interface SidebarNavProps {
  children: React.ReactNode;
  className?: string;
}

interface SidebarNavItemProps {
  href?: string;
  onClick?: () => void;
  isActive?: boolean;
  icon?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}

export const Sidebar: React.FC<SidebarProps> = ({ 
  isOpen = true, 
  onClose, 
  children, 
  className,
  variant = 'desktop'
}) => {
  if (variant === 'mobile') {
    return (
      <Transition show={isOpen} as={Fragment}>
        <Dialog as="div" className="relative z-50 lg:hidden" onClose={onClose || (() => {})}>
          <Transition.Child
            as={Fragment}
            enter="transition-opacity ease-linear duration-300"
            enterFrom="opacity-0"
            enterTo="opacity-100"
            leave="transition-opacity ease-linear duration-300"
            leaveFrom="opacity-100"
            leaveTo="opacity-0"
          >
            <div className="fixed inset-0 bg-gray-900/80" />
          </Transition.Child>

          <div className="fixed inset-0 flex">
            <Transition.Child
              as={Fragment}
              enter="transition ease-in-out duration-300 transform"
              enterFrom="-translate-x-full"
              enterTo="translate-x-0"
              leave="transition ease-in-out duration-300 transform"
              leaveFrom="translate-x-0"
              leaveTo="-translate-x-full"
            >
              <Dialog.Panel className={cn(
                "relative mr-16 flex w-full max-w-xs flex-1 flex-col bg-white",
                className
              )}>
                {children}
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </Dialog>
      </Transition>
    );
  }

  return (
    <div className={cn(
      "hidden lg:fixed lg:inset-y-0 lg:z-40 lg:flex lg:w-64 lg:flex-col",
      className
    )}>
      <div className="flex grow flex-col overflow-y-auto bg-white border-r border-gray-200">
        {children}
      </div>
    </div>
  );
};

export const SidebarHeader: React.FC<SidebarHeaderProps> = ({ children, className }) => (
  <div className={cn("flex h-16 shrink-0 items-center px-6 border-b border-gray-200", className)}>
    {children}
  </div>
);

export const SidebarContent: React.FC<SidebarContentProps> = ({ children, className }) => (
  <div className={cn("flex flex-1 flex-col py-4", className)}>
    {children}
  </div>
);

export const SidebarNav: React.FC<SidebarNavProps> = ({ children, className }) => (
  <nav className={cn("flex-1 space-y-1 px-3", className)}>
    {children}
  </nav>
);

export const SidebarNavItem: React.FC<SidebarNavItemProps> = ({ 
  href, 
  onClick, 
  isActive = false, 
  icon, 
  children, 
  className 
}) => {
  const baseClasses = "group flex items-center px-3 py-2 text-sm font-medium rounded-lg transition-colors";
  const activeClasses = isActive 
    ? "bg-primary-50 text-primary-700 border-r-2 border-primary-700"
    : "text-gray-700 hover:bg-gray-50 hover:text-gray-900";

  const Component = href ? 'a' : 'button';
  const props = href ? { href } : { onClick };

  return (
    <Component
      className={cn(baseClasses, activeClasses, className)}
      {...props}
    >
      {icon && (
        <span className={cn(
          "mr-3 flex-shrink-0 h-5 w-5",
          isActive ? "text-primary-600" : "text-gray-400 group-hover:text-gray-600"
        )}>
          {icon}
        </span>
      )}
      <span className="truncate">{children}</span>
    </Component>
  );
};