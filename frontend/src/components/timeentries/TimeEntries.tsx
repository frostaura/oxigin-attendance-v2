import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  CircularProgress,
  Alert,
} from '@mui/material';
import { timeEntryApi } from '../../services/api';
import { TimeEntry, TimeEntryStatus } from '../../types/timeEntry';
import { formatDateTime } from '../../utils/dateTime';

const TimeEntries: React.FC = () => {
  const [timeEntries, setTimeEntries] = useState<TimeEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTimeEntries = async () => {
      try {
        setLoading(true);
        const entries = await timeEntryApi.getTimeEntries();
        setTimeEntries(entries);
      } catch (err) {
        setError('Failed to load time entries');
        console.error('Error fetching time entries:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTimeEntries();
  }, []);

  const getStatusChip = (status: TimeEntryStatus) => {
    const statusConfig = {
      [TimeEntryStatus.Active]: { color: 'success' as const, label: 'Active' },
      [TimeEntryStatus.Completed]: { color: 'primary' as const, label: 'Completed' },
      [TimeEntryStatus.Cancelled]: { color: 'error' as const, label: 'Cancelled' },
    };

    const config = statusConfig[status] || { color: 'default' as const, label: 'Unknown' };
    return <Chip size="small" color={config.color} label={config.label} />;
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Time Entries
      </Typography>
      
      <Card>
        <CardContent>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Clock In</TableCell>
                  <TableCell>Clock Out</TableCell>
                  <TableCell>Break Time</TableCell>
                  <TableCell>Total Hours</TableCell>
                  <TableCell>Overtime</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Notes</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {timeEntries.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      <Typography variant="body2" color="text.secondary">
                        No time entries found
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  timeEntries.map((entry) => (
                    <TableRow key={entry.id}>
                      <TableCell>{formatDateTime(entry.clockInTime, 'MMM DD, YYYY')}</TableCell>
                      <TableCell>{formatDateTime(entry.clockInTime, 'HH:mm')}</TableCell>
                      <TableCell>
                        {entry.clockOutTime ? formatDateTime(entry.clockOutTime, 'HH:mm') : '-'}
                      </TableCell>
                      <TableCell>
                        {entry.breakTime || '-'}
                      </TableCell>
                      <TableCell>
                        {entry.totalHours || '-'}
                      </TableCell>
                      <TableCell>
                        {entry.overtimeHours || '-'}
                      </TableCell>
                      <TableCell>{getStatusChip(entry.status)}</TableCell>
                      <TableCell>{entry.notes || '-'}</TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Box>
  );
};

export default TimeEntries;