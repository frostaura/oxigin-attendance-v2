import React from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '../utils/cn';

const buttonVariants = cva(
  'inline-flex items-center justify-center whitespace-nowrap rounded-lg text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-primary-500 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        primary: 'bg-primary-600 text-white shadow hover:bg-primary-700',
        secondary: 'bg-white text-gray-700 border border-gray-300 shadow-sm hover:bg-gray-50',
        destructive: 'bg-error-600 text-white shadow hover:bg-error-700',
        outline: 'border border-gray-300 bg-white text-gray-700 shadow-sm hover:bg-gray-50',
        ghost: 'text-gray-700 hover:bg-gray-100',
        link: 'text-primary-600 underline-offset-4 hover:underline',
      },
      size: {
        sm: 'h-8 px-3 text-xs',
        md: 'h-9 px-4 text-sm',
        lg: 'h-10 px-4 text-sm',
        xl: 'h-11 px-5 text-base',
        '2xl': 'h-12 px-6 text-base',
        icon: 'h-9 w-9',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
    },
  }
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
  startIcon?: React.ReactNode;
  endIcon?: React.ReactNode;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, startIcon, endIcon, children, asChild = false, ...props }, ref) => {
    return (
      <button
        className={cn(buttonVariants({ variant, size }), className)}
        ref={ref}
        {...props}
      >
        {startIcon && <span className="mr-2">{startIcon}</span>}
        {children}
        {endIcon && <span className="ml-2">{endIcon}</span>}
      </button>
    );
  }
);
Button.displayName = 'Button';

export { Button, buttonVariants };