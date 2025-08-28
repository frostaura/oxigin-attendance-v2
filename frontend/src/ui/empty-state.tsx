import React from 'react';
import { Button } from './button';
import { Typography } from './typography';
import { cn } from '../utils/utils';

interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
    variant?: 'primary' | 'secondary' | 'outline';
  };
  className?: string;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  action,
  className
}) => {
  return (
    <div className={cn('flex flex-col items-center justify-center py-12 px-4 text-center', className)}>
      {icon && (
        <div className="mb-4 text-gray-400">
          {icon}
        </div>
      )}
      <Typography variant="h4" className="text-gray-900 mb-2">
        {title}
      </Typography>
      {description && (
        <Typography variant="body1" className="text-gray-500 mb-6 max-w-md">
          {description}
        </Typography>
      )}
      {action && (
        <Button
          variant={action.variant || 'primary'}
          onClick={action.onClick}
        >
          {action.label}
        </Button>
      )}
    </div>
  );
};

// Pre-built empty states for common scenarios
export const EmptyTimeEntries: React.FC<{ onAddEntry?: () => void }> = ({ onAddEntry }) => (
  <EmptyState
    icon={<ClockEmptyIcon className="h-16 w-16" />}
    title="No Time Entries"
    description="You haven't recorded any time entries yet. Start tracking your time to see your activity here."
    action={onAddEntry ? {
      label: "Clock In",
      onClick: onAddEntry,
      variant: "primary"
    } : undefined}
  />
);

export const EmptyReports: React.FC<{ onGenerate?: () => void }> = ({ onGenerate }) => (
  <EmptyState
    icon={<ChartEmptyIcon className="h-16 w-16" />}
    title="No Reports Available"
    description="No reports have been generated yet. Create your first report to view analytics and insights."
    action={onGenerate ? {
      label: "Generate Report",
      onClick: onGenerate,
      variant: "primary"
    } : undefined}
  />
);

export const EmptyEmployees: React.FC<{ onAddEmployee?: () => void }> = ({ onAddEmployee }) => (
  <EmptyState
    icon={<UsersEmptyIcon className="h-16 w-16" />}
    title="No Employees Found"
    description="No employees match your current search criteria. Try adjusting your filters or add a new employee."
    action={onAddEmployee ? {
      label: "Add Employee",
      onClick: onAddEmployee,
      variant: "primary"
    } : undefined}
  />
);

export const EmptySearch: React.FC<{ searchTerm: string; onClear?: () => void }> = ({ searchTerm, onClear }) => (
  <EmptyState
    icon={<SearchEmptyIcon className="h-16 w-16" />}
    title="No Results Found"
    description={`No results found for "${searchTerm}". Try adjusting your search terms or filters.`}
    action={onClear ? {
      label: "Clear Search",
      onClick: onClear,
      variant: "outline"
    } : undefined}
  />
);

export const EmptyData: React.FC<{ onRefresh?: () => void }> = ({ onRefresh }) => (
  <EmptyState
    icon={<DatabaseEmptyIcon className="h-16 w-16" />}
    title="No Data Available"
    description="There's no data to display at the moment. Check back later or try refreshing the page."
    action={onRefresh ? {
      label: "Refresh",
      onClick: onRefresh,
      variant: "outline"
    } : undefined}
  />
);

// Empty state icons with beautiful illustrations
const ClockEmptyIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 64 64" fill="none">
    <circle cx="32" cy="32" r="28" stroke="currentColor" strokeWidth="2" strokeDasharray="8 4"/>
    <circle cx="32" cy="32" r="20" stroke="currentColor" strokeWidth="1.5" opacity="0.5"/>
    <path d="M32 22v10l6 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <circle cx="32" cy="32" r="2" fill="currentColor"/>
    <path d="M16 8l-2 2m36-2l2 2M8 32H4m56 0h-4M16 56l-2-2m36 2l2-2" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" opacity="0.3"/>
  </svg>
);

const ChartEmptyIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 64 64" fill="none">
    <rect x="8" y="8" width="48" height="48" rx="4" stroke="currentColor" strokeWidth="2" strokeDasharray="6 3"/>
    <rect x="16" y="38" width="6" height="10" fill="currentColor" opacity="0.3"/>
    <rect x="26" y="28" width="6" height="20" fill="currentColor" opacity="0.5"/>
    <rect x="36" y="34" width="6" height="14" fill="currentColor" opacity="0.4"/>
    <rect x="46" y="20" width="6" height="28" fill="currentColor" opacity="0.6"/>
    <path d="M16 16h32M16 20h24M16 24h28" stroke="currentColor" strokeWidth="1" opacity="0.2"/>
  </svg>
);

const UsersEmptyIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 64 64" fill="none">
    <circle cx="32" cy="22" r="8" stroke="currentColor" strokeWidth="2" strokeDasharray="4 2"/>
    <ellipse cx="32" cy="45" rx="16" ry="8" stroke="currentColor" strokeWidth="2" strokeDasharray="4 2"/>
    <circle cx="20" cy="26" r="6" stroke="currentColor" strokeWidth="1.5" opacity="0.5"/>
    <ellipse cx="20" cy="44" rx="12" ry="6" stroke="currentColor" strokeWidth="1.5" opacity="0.5"/>
    <circle cx="44" cy="26" r="6" stroke="currentColor" strokeWidth="1.5" opacity="0.5"/>
    <ellipse cx="44" cy="44" rx="12" ry="6" stroke="currentColor" strokeWidth="1.5" opacity="0.5"/>
  </svg>
);

const SearchEmptyIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 64 64" fill="none">
    <circle cx="28" cy="28" r="16" stroke="currentColor" strokeWidth="2" strokeDasharray="6 3"/>
    <path d="M40 40l16 16" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeDasharray="6 3"/>
    <circle cx="28" cy="28" r="8" stroke="currentColor" strokeWidth="1.5" opacity="0.3"/>
    <path d="M20 20l8 8M36 20l-8 8" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" opacity="0.5"/>
  </svg>
);

const DatabaseEmptyIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 64 64" fill="none">
    <ellipse cx="32" cy="16" rx="20" ry="6" stroke="currentColor" strokeWidth="2" strokeDasharray="4 2"/>
    <path d="M12 16v12c0 3.5 9 6 20 6s20-2.5 20-6V16" stroke="currentColor" strokeWidth="2" strokeDasharray="4 2"/>
    <path d="M12 28v12c0 3.5 9 6 20 6s20-2.5 20-6V28" stroke="currentColor" strokeWidth="2" strokeDasharray="4 2"/>
    <path d="M12 40v8c0 3.5 9 6 20 6s20-2.5 20-6v-8" stroke="currentColor" strokeWidth="2" strokeDasharray="4 2"/>
    <ellipse cx="32" cy="28" rx="16" ry="4" stroke="currentColor" strokeWidth="1" opacity="0.3"/>
    <ellipse cx="32" cy="40" rx="16" ry="4" stroke="currentColor" strokeWidth="1" opacity="0.3"/>
  </svg>
);