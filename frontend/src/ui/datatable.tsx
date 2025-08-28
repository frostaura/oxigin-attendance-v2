import React, { useState, useMemo } from 'react';
import { cn } from '../utils/utils';
import { Button } from './button';
import { Input } from './input';
import { Typography } from './typography';
import { ArrowLeftIcon, ArrowRightIcon } from './icons';

// SearchIcon definition
const SearchIcon: React.FC<{ className?: string }> = ({ className = "w-5 h-5" }) => (
  <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="m21 21-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
  </svg>
);

export interface Column<T = any> {
  key: string;
  title: string;
  dataIndex: keyof T;
  width?: string | number;
  sortable?: boolean;
  filterable?: boolean;
  render?: (value: any, record: T, index: number) => React.ReactNode;
  align?: 'left' | 'center' | 'right';
}

export interface DataTableProps<T = any> {
  columns: Column<T>[];
  data: T[];
  loading?: boolean;
  pagination?: {
    current: number;
    pageSize: number;
    total: number;
    showSizeChanger?: boolean;
    showQuickJumper?: boolean;
    showTotal?: (total: number, range: [number, number]) => React.ReactNode;
  };
  searchable?: boolean;
  searchPlaceholder?: string;
  rowKey?: keyof T | ((record: T) => string);
  onRow?: (record: T, index: number) => React.HTMLAttributes<HTMLTableRowElement>;
  className?: string;
  onPaginationChange?: (page: number, pageSize: number) => void;
  onSearch?: (value: string) => void;
  onSort?: (key: string, direction: 'asc' | 'desc' | null) => void;
  emptyText?: string;
}

export function DataTable<T extends Record<string, any>>({
  columns,
  data,
  loading = false,
  pagination,
  searchable = true,
  searchPlaceholder = "Search...",
  rowKey = 'id',
  onRow,
  className,
  onPaginationChange,
  onSearch,
  onSort,
  emptyText = "No data available"
}: DataTableProps<T>) {
  const [searchValue, setSearchValue] = useState('');
  const [sortState, setSortState] = useState<{ key: string; direction: 'asc' | 'desc' | null }>({
    key: '',
    direction: null
  });

  // Filter and sort data locally if no external handlers are provided
  const processedData = useMemo(() => {
    let filtered = data;

    // Search
    if (searchValue && !onSearch) {
      filtered = data.filter((record) =>
        columns.some((column) => {
          const value = record[column.dataIndex];
          return value && value.toString().toLowerCase().includes(searchValue.toLowerCase());
        })
      );
    }

    // Sort
    if (sortState.direction && !onSort) {
      filtered = [...filtered].sort((a, b) => {
        const aVal = a[sortState.key];
        const bVal = b[sortState.key];
        
        if (sortState.direction === 'asc') {
          return aVal > bVal ? 1 : -1;
        }
        return aVal < bVal ? 1 : -1;
      });
    }

    return filtered;
  }, [data, searchValue, sortState, columns, onSearch, onSort]);

  const handleSearch = (value: string) => {
    setSearchValue(value);
    onSearch?.(value);
  };

  const handleSort = (key: string) => {
    const newDirection = 
      sortState.key === key && sortState.direction === 'asc' 
        ? 'desc' 
        : sortState.key === key && sortState.direction === 'desc'
        ? null
        : 'asc';

    setSortState({ key, direction: newDirection });
    onSort?.(key, newDirection);
  };

  const getRowKey = (record: T, index: number) => {
    if (typeof rowKey === 'function') {
      return rowKey(record);
    }
    return record[rowKey]?.toString() || index.toString();
  };

  const renderPagination = () => {
    if (!pagination) return null;

    const { current, pageSize, total } = pagination;
    const totalPages = Math.ceil(total / pageSize);
    const startItem = (current - 1) * pageSize + 1;
    const endItem = Math.min(current * pageSize, total);

    return (
      <div className="flex items-center justify-between px-6 py-3 bg-white border-t border-gray-200">
        <div className="flex items-center text-sm text-gray-700">
          Showing {startItem} to {endItem} of {total} results
        </div>
        <div className="flex items-center space-x-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => onPaginationChange?.(current - 1, pageSize)}
            disabled={current === 1}
          >
            <ArrowLeftIcon className="h-4 w-4" />
            Previous
          </Button>
          
          <div className="flex items-center space-x-1">
            {Array.from({ length: Math.min(5, totalPages) }, (_, index) => {
              let pageNum: number;
              if (totalPages <= 5) {
                pageNum = index + 1;
              } else if (current <= 3) {
                pageNum = index + 1;
              } else if (current >= totalPages - 2) {
                pageNum = totalPages - 4 + index;
              } else {
                pageNum = current - 2 + index;
              }

              return (
                <Button
                  key={pageNum}
                  variant={current === pageNum ? 'primary' : 'outline'}
                  size="sm"
                  onClick={() => onPaginationChange?.(pageNum, pageSize)}
                  className="w-10"
                >
                  {pageNum}
                </Button>
              );
            })}
          </div>

          <Button
            variant="outline"
            size="sm"
            onClick={() => onPaginationChange?.(current + 1, pageSize)}
            disabled={current === totalPages}
          >
            Next
            <ArrowRightIcon className="h-4 w-4" />
          </Button>
        </div>
      </div>
    );
  };

  if (loading) {
    return (
      <div className={cn('bg-white rounded-lg border border-gray-200', className)}>
        <div className="p-6">
          <div className="animate-pulse space-y-4">
            {Array.from({ length: 5 }, (_, index) => (
              <div key={index} className="h-12 bg-gray-200 rounded" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={cn('bg-white rounded-lg border border-gray-200 shadow-sm', className)}>
      {/* Search Bar */}
      {searchable && (
        <div className="p-4 border-b border-gray-200">
          <div className="relative max-w-sm">
            <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder={searchPlaceholder}
              value={searchValue}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>
      )}

      {/* Table */}
      <div className="overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  className={cn(
                    'px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider',
                    column.align === 'center' && 'text-center',
                    column.align === 'right' && 'text-right',
                    column.sortable && 'cursor-pointer hover:bg-gray-100 user-select-none'
                  )}
                  style={{ width: column.width }}
                  onClick={() => column.sortable && handleSort(column.key)}
                >
                  <div className="flex items-center space-x-1">
                    <span>{column.title}</span>
                    {column.sortable && (
                      <div className="flex flex-col">
                        <div className={cn(
                          'w-0 h-0 border-l-4 border-r-4 border-b-4 border-transparent mb-1',
                          sortState.key === column.key && sortState.direction === 'asc'
                            ? 'border-b-gray-900'
                            : 'border-b-gray-300'
                        )} />
                        <div className={cn(
                          'w-0 h-0 border-l-4 border-r-4 border-t-4 border-transparent',
                          sortState.key === column.key && sortState.direction === 'desc'
                            ? 'border-t-gray-900'
                            : 'border-t-gray-300'
                        )} />
                      </div>
                    )}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {processedData.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-6 py-12 text-center">
                  <div className="text-gray-500">
                    <Typography variant="body2">{emptyText}</Typography>
                  </div>
                </td>
              </tr>
            ) : (
              processedData.map((record, index) => (
                <tr
                  key={getRowKey(record, index)}
                  className="hover:bg-gray-50 transition-colors duration-150"
                  {...(onRow?.(record, index) || {})}
                >
                  {columns.map((column) => (
                    <td
                      key={column.key}
                      className={cn(
                        'px-6 py-4 whitespace-nowrap text-sm text-gray-900',
                        column.align === 'center' && 'text-center',
                        column.align === 'right' && 'text-right'
                      )}
                    >
                      {column.render
                        ? column.render(record[column.dataIndex], record, index)
                        : record[column.dataIndex]}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {renderPagination()}
    </div>
  );
}