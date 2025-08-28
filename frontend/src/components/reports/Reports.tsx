import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Paper,
  Alert,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs, { Dayjs } from 'dayjs';
import { timeEntryApi } from '../../services/api';
import { TimeReport } from '../../types/timeEntry';

const Reports: React.FC = () => {
  const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
  const [endDate, setEndDate] = useState<Dayjs | null>(dayjs().endOf('month'));
  const [report, setReport] = useState<TimeReport | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleGenerateReport = async () => {
    if (!startDate || !endDate) {
      setError('Please select both start and end dates');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const reportData = await timeEntryApi.getTimeReport(
        startDate.format('YYYY-MM-DD'),
        endDate.format('YYYY-MM-DD')
      );
      setReport(reportData);
    } catch (err) {
      setError('Failed to generate report');
      console.error('Error generating report:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Reports
      </Typography>
      
      {/* Date Range Selection */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Generate Time Report
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
            <Box sx={{ minWidth: 200 }}>
              <DatePicker
                label="Start Date"
                value={startDate}
                onChange={(newValue) => setStartDate(newValue)}
                slotProps={{ textField: { fullWidth: true } }}
              />
            </Box>
            <Box sx={{ minWidth: 200 }}>
              <DatePicker
                label="End Date"
                value={endDate}
                onChange={(newValue) => setEndDate(newValue)}
                slotProps={{ textField: { fullWidth: true } }}
              />
            </Box>
            <Button
              variant="contained"
              onClick={handleGenerateReport}
              disabled={loading}
            >
              {loading ? 'Generating...' : 'Generate Report'}
            </Button>
          </Box>
          
          {error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {error}
            </Alert>
          )}
        </CardContent>
      </Card>

      {/* Report Results */}
      {report && (
        <Box>
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Summary
              </Typography>
              <Box>
                <Typography variant="body1">
                  <strong>Total Hours:</strong> {report.totalWorkedHours}
                </Typography>
                <Typography variant="body1">
                  <strong>Overtime Hours:</strong> {report.totalOvertimeHours}
                </Typography>
                <Typography variant="body1">
                  <strong>Total Days:</strong> {report.totalDaysWorked}
                </Typography>
              </Box>
            </CardContent>
          </Card>
          
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Daily Breakdown
              </Typography>
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
                      <TableCell>Notes</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {report.timeEntries.map((entry) => (
                      <TableRow key={entry.id}>
                        <TableCell>{dayjs(entry.clockInTime).format('MMM DD, YYYY')}</TableCell>
                        <TableCell>{dayjs(entry.clockInTime).format('HH:mm')}</TableCell>
                        <TableCell>
                          {entry.clockOutTime ? dayjs(entry.clockOutTime).format('HH:mm') : '-'}
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
                        <TableCell>{entry.notes || '-'}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Box>
      )}
    </Box>
  );
};

export default Reports;