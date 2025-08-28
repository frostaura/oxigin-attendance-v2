import dayjs from 'dayjs';
import duration from 'dayjs/plugin/duration';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(duration);
dayjs.extend(relativeTime);

export const formatDate = (date: string | Date, format = 'YYYY-MM-DD'): string => {
  return dayjs(date).format(format);
};

export const formatDateTime = (date: string | Date, format = 'YYYY-MM-DD HH:mm:ss'): string => {
  return dayjs(date).format(format);
};

export const formatTime = (date: string | Date, format = 'HH:mm'): string => {
  return dayjs(date).format(format);
};

export const formatDuration = (durationStr?: string): string => {
  if (!durationStr) return '00:00';
  
  try {
    // Parse .NET TimeSpan format (e.g., "08:30:00" or "1.08:30:00")
    const parts = durationStr.split(':');
    let hours = 0;
    let minutes = 0;
    
    if (parts.length >= 2) {
      const hoursPart = parts[0];
      if (hoursPart.includes('.')) {
        // Handle format like "1.08:30:00" (days.hours:minutes:seconds)
        const dayHoursParts = hoursPart.split('.');
        const days = parseInt(dayHoursParts[0]) || 0;
        hours = (days * 24) + (parseInt(dayHoursParts[1]) || 0);
      } else {
        hours = parseInt(hoursPart) || 0;
      }
      minutes = parseInt(parts[1]) || 0;
    }
    
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  } catch {
    return '00:00';
  }
};

export const getRelativeTime = (date: string | Date): string => {
  return dayjs(date).fromNow();
};

export const isToday = (date: string | Date): boolean => {
  return dayjs(date).isSame(dayjs(), 'day');
};

export const calculateTimeDifference = (start: string | Date, end: string | Date): string => {
  const startTime = dayjs(start);
  const endTime = dayjs(end);
  const diff = endTime.diff(startTime);
  const duration = dayjs.duration(diff);
  
  const hours = Math.floor(duration.asHours());
  const minutes = duration.minutes();
  
  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
};

export const getCurrentDateTime = (): string => {
  return dayjs().toISOString();
};

export const getTodayDateString = (): string => {
  return dayjs().format('YYYY-MM-DD');
};

export const getStartOfWeek = (): string => {
  return dayjs().startOf('week').format('YYYY-MM-DD');
};

export const getEndOfWeek = (): string => {
  return dayjs().endOf('week').format('YYYY-MM-DD');
};

export const getStartOfMonth = (): string => {
  return dayjs().startOf('month').format('YYYY-MM-DD');
};

export const getEndOfMonth = (): string => {
  return dayjs().endOf('month').format('YYYY-MM-DD');
};