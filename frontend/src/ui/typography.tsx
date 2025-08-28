import React from 'react';
import { cn } from '../utils/cn';

export interface TypographyProps extends React.HTMLAttributes<HTMLElement> {
  variant?: 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6' | 'body1' | 'body2' | 'caption' | 'overline';
  component?: keyof React.JSX.IntrinsicElements;
  color?: 'primary' | 'secondary' | 'textPrimary' | 'textSecondary' | 'error' | 'warning' | 'info' | 'success';
}

const variantClasses = {
  h1: 'text-5xl font-semibold leading-tight tracking-tight text-gray-900',
  h2: 'text-4xl font-semibold leading-tight tracking-tight text-gray-900',
  h3: 'text-3xl font-semibold leading-tight text-gray-900',
  h4: 'text-2xl font-semibold leading-tight text-gray-900',
  h5: 'text-xl font-semibold leading-tight text-gray-900',
  h6: 'text-lg font-semibold leading-tight text-gray-900',
  body1: 'text-base leading-6 text-gray-900',
  body2: 'text-sm leading-5 text-gray-900',
  caption: 'text-xs leading-4 text-gray-600',
  overline: 'text-xs font-medium uppercase tracking-wide text-gray-600',
};

const colorClasses = {
  primary: 'text-primary-600',
  secondary: 'text-gray-600',
  textPrimary: 'text-gray-900',
  textSecondary: 'text-gray-600',
  error: 'text-error-600',
  warning: 'text-warning-600',
  info: 'text-primary-600',
  success: 'text-success-600',
};

const defaultComponents = {
  h1: 'h1',
  h2: 'h2',
  h3: 'h3',
  h4: 'h4',
  h5: 'h5',
  h6: 'h6',
  body1: 'p',
  body2: 'p',
  caption: 'span',
  overline: 'span',
} as const;

export const Typography: React.FC<TypographyProps> = ({
  variant = 'body1',
  component,
  color,
  className,
  children,
  ...props
}) => {
  const Component = (component || defaultComponents[variant]) as keyof React.JSX.IntrinsicElements;
  
  return React.createElement(
    Component,
    {
      className: cn(
        variantClasses[variant],
        color && colorClasses[color],
        className
      ),
      ...props,
    },
    children
  );
};