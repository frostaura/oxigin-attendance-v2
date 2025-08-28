import React from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '../utils/utils';

// Skeleton variants
const skeletonVariants = cva(
  'animate-pulse bg-gray-200 rounded',
  {
    variants: {
      variant: {
        default: 'bg-gray-200',
        card: 'bg-gray-100 border border-gray-200',
        text: 'bg-gray-200 h-4',
        avatar: 'bg-gray-200 rounded-full',
        button: 'bg-gray-200 rounded-md',
      },
      size: {
        sm: 'h-4',
        md: 'h-6',
        lg: 'h-8',
        xl: 'h-10',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'md',
    },
  }
);

export interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement>, VariantProps<typeof skeletonVariants> {
  width?: string | number;
  height?: string | number;
  circle?: boolean;
  count?: number;
}

export const Skeleton: React.FC<SkeletonProps> = ({ 
  className, 
  variant, 
  size, 
  width,
  height,
  circle,
  count = 1,
  style,
  ...props 
}) => {
  const skeletonStyle = {
    width,
    height,
    ...style,
  };

  const skeletonClassName = cn(
    skeletonVariants({ variant, size }),
    circle && 'rounded-full',
    className
  );

  if (count === 1) {
    return (
      <div
        className={skeletonClassName}
        style={skeletonStyle}
        {...props}
      />
    );
  }

  return (
    <div className="space-y-2">
      {Array.from({ length: count }, (_, index) => (
        <div
          key={index}
          className={skeletonClassName}
          style={skeletonStyle}
          {...props}
        />
      ))}
    </div>
  );
};

// Skeleton component compositions for common patterns
export const SkeletonCard: React.FC<{ className?: string }> = ({ className }) => (
  <div className={cn('p-6 border border-gray-200 rounded-lg bg-white', className)}>
    <div className="space-y-4">
      <Skeleton variant="text" width="60%" />
      <Skeleton variant="text" count={3} />
      <div className="flex items-center space-x-2">
        <Skeleton variant="avatar" width={32} height={32} circle />
        <Skeleton variant="text" width="40%" />
      </div>
    </div>
  </div>
);

export const SkeletonTable: React.FC<{ rows?: number; columns?: number; className?: string }> = ({ 
  rows = 5, 
  columns = 4, 
  className 
}) => (
  <div className={cn('w-full', className)}>
    {/* Header */}
    <div className="grid grid-cols-4 gap-4 p-4 border-b border-gray-200">
      {Array.from({ length: columns }, (_, index) => (
        <Skeleton key={`header-${index}`} variant="text" width="80%" />
      ))}
    </div>
    {/* Rows */}
    <div className="divide-y divide-gray-200">
      {Array.from({ length: rows }, (_, rowIndex) => (
        <div key={`row-${rowIndex}`} className="grid grid-cols-4 gap-4 p-4">
          {Array.from({ length: columns }, (_, colIndex) => (
            <Skeleton key={`cell-${rowIndex}-${colIndex}`} variant="text" />
          ))}
        </div>
      ))}
    </div>
  </div>
);

export const SkeletonDashboard: React.FC<{ className?: string }> = ({ className }) => (
  <div className={cn('space-y-6', className)}>
    {/* Header */}
    <div className="flex items-center justify-between">
      <Skeleton variant="text" width={200} height={32} />
      <Skeleton variant="button" width={120} height={40} />
    </div>
    
    {/* Stats Cards */}
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      {Array.from({ length: 4 }, (_, index) => (
        <div key={index} className="p-6 border border-gray-200 rounded-lg bg-white">
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <Skeleton variant="text" width="60%" />
              <Skeleton variant="avatar" width={24} height={24} />
            </div>
            <Skeleton variant="text" width="40%" size="xl" />
            <Skeleton variant="text" width="80%" size="sm" />
          </div>
        </div>
      ))}
    </div>
    
    {/* Content Area */}
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div className="lg:col-span-2">
        <SkeletonCard />
      </div>
      <div className="space-y-4">
        <SkeletonCard />
        <SkeletonCard />
      </div>
    </div>
  </div>
);

export const SkeletonList: React.FC<{ items?: number; className?: string }> = ({ 
  items = 5, 
  className 
}) => (
  <div className={cn('space-y-3', className)}>
    {Array.from({ length: items }, (_, index) => (
      <div key={index} className="flex items-center space-x-4 p-4 border border-gray-200 rounded-lg">
        <Skeleton variant="avatar" width={40} height={40} circle />
        <div className="flex-1 space-y-2">
          <Skeleton variant="text" width="60%" />
          <Skeleton variant="text" width="40%" size="sm" />
        </div>
        <Skeleton variant="button" width={80} height={32} />
      </div>
    ))}
  </div>
);