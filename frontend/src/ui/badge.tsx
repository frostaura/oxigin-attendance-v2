import React from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '../utils/utils';

const badgeVariants = cva(
  'inline-flex items-center rounded-full text-xs font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
  {
    variants: {
      variant: {
        default: 'bg-primary-600 text-white shadow hover:bg-primary-700',
        secondary: 'bg-gray-100 text-gray-900 hover:bg-gray-200',
        success: 'bg-success-100 text-success-800 border border-success-200',
        warning: 'bg-warning-100 text-warning-800 border border-warning-200',
        error: 'bg-error-100 text-error-800 border border-error-200',
        outline: 'border border-gray-200 text-gray-700 hover:bg-gray-50',
      },
      size: {
        sm: 'px-2 py-1 text-xs',
        md: 'px-2.5 py-1.5 text-sm',
        lg: 'px-3 py-2 text-base',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'sm',
    },
  }
);

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {
  label: string;
}

function Badge({ className, variant, size, label, ...props }: BadgeProps) {
  return (
    <div className={cn(badgeVariants({ variant, size }), className)} {...props}>
      {label}
    </div>
  );
}

export { Badge, badgeVariants };