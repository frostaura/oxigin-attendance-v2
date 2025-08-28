import React, { useEffect, useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Alert,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
} from '@mui/material';
import {
  PlayArrow as ClockInIcon,
  Stop as ClockOutIcon,
  Schedule as ScheduleIcon,
  Today as TodayIcon,
} from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import {
  setLoading,
  setError,
  setCurrentTimeEntry,
  addTimeEntry,
  updateTimeEntry,
  clearError,
} from '../../store/timeEntrySlice';
import { timeEntryApi } from '../../services/api';
import { formatTime, formatDuration, getTodayDateString } from '../../utils/dateTime';
import { TimeEntryStatus } from '../../types/timeEntry';

const Dashboard: React.FC = () => {
  const [clockOutDialog, setClockOutDialog] = useState(false);
  const [notes, setNotes] = useState('');
  const [breakTime, setBreakTime] = useState('');
  
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { currentTimeEntry, loading, error } = useAppSelector((state) => state.timeEntry);

  useEffect(() => {
    const loadCurrentTimeEntry = async () => {
      try {
        dispatch(setLoading(true));
        const timeEntry = await timeEntryApi.getActiveTimeEntry();
        dispatch(setCurrentTimeEntry(timeEntry));
      } catch (error: any) {
        dispatch(setError(error.response?.data?.message || 'Failed to load time entry'));
      } finally {
        dispatch(setLoading(false));
      }
    };

    loadCurrentTimeEntry();
  }, [dispatch]);

  const handleClockIn = async () => {
    try {
      dispatch(setLoading(true));
      dispatch(clearError());
      
      const timeEntry = await timeEntryApi.clockIn({
        location: 'Office', // You can make this dynamic
      });
      
      dispatch(setCurrentTimeEntry(timeEntry));
      dispatch(addTimeEntry(timeEntry));
    } catch (error: any) {
      dispatch(setError(error.response?.data?.message || 'Failed to clock in'));
    } finally {
      dispatch(setLoading(false));
    }
  };

  const handleClockOutClick = () => {
    setNotes('');
    setBreakTime('');
    setClockOutDialog(true);
  };

  const handleClockOut = async () => {
    if (!currentTimeEntry) return;

    try {
      dispatch(setLoading(true));
      dispatch(clearError());
      
      const timeEntry = await timeEntryApi.clockOut({
        timeEntryId: currentTimeEntry.id,
        notes: notes || undefined,
        breakTime: breakTime || undefined,
      });
      
      dispatch(setCurrentTimeEntry(null));
      dispatch(updateTimeEntry(timeEntry));
      setClockOutDialog(false);
    } catch (error: any) {
      dispatch(setError(error.response?.data?.message || 'Failed to clock out'));
    } finally {
      dispatch(setLoading(false));
    }
  };

  const isCurrentlyClockedIn = currentTimeEntry?.status === TimeEntryStatus.Active;
  const currentTime = new Date();

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Welcome back, {user?.firstName}!
      </Typography>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => dispatch(clearError())}>
          {error}
        </Alert>
      )}

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        {/* Clock In/Out Card */}
        <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
          <Paper sx={{ p: 3, textAlign: 'center', minHeight: 200, flex: '1 1 300px' }} data-onboarding-target="time-tracking-card">
            <Typography variant="h6" gutterBottom>
              Time Tracking
            </Typography>
            
            {isCurrentlyClockedIn ? (
              <Box>
                <Chip 
                  label="Currently Clocked In" 
                  color="success" 
                  sx={{ mb: 2 }}
                  icon={<ScheduleIcon />}
                />
                <Typography variant="body1" sx={{ mb: 1 }}>
                  Clock In Time: {formatTime(currentTimeEntry.clockInTime)}
                </Typography>
                <Typography variant="body1" sx={{ mb: 3 }}>
                  Current Time: {formatTime(currentTime)}
                </Typography>
                <Button
                  variant="contained"
                  color="error"
                  size="large"
                  onClick={handleClockOutClick}
                  disabled={loading}
                  startIcon={<ClockOutIcon />}
                >
                  Clock Out
                </Button>
              </Box>
            ) : (
              <Box>
                <Chip 
                  label="Not Clocked In" 
                  color="default" 
                  sx={{ mb: 2 }}
                />
                <Typography variant="body1" sx={{ mb: 3 }}>
                  Current Time: {formatTime(currentTime)}
                </Typography>
                <Button
                  variant="contained"
                  color="primary"
                  size="large"
                  onClick={handleClockIn}
                  disabled={loading}
                  startIcon={<ClockInIcon />}
                >
                  Clock In
                </Button>
              </Box>
            )}
          </Paper>

          {/* Today's Summary Card */}
          <Paper sx={{ p: 3, minHeight: 200, flex: '1 1 300px' }}>
            <Typography variant="h6" gutterBottom>
              <TodayIcon sx={{ mr: 1, verticalAlign: 'bottom' }} />
              Today's Summary
            </Typography>
            
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="textSecondary">
                Date: {getTodayDateString()}
              </Typography>
              
              {currentTimeEntry && (
                <>
                  <Typography variant="body2" sx={{ mt: 1 }}>
                    Clock In: {formatTime(currentTimeEntry.clockInTime)}
                  </Typography>
                  
                  {currentTimeEntry.clockOutTime && (
                    <>
                      <Typography variant="body2">
                        Clock Out: {formatTime(currentTimeEntry.clockOutTime)}
                      </Typography>
                      <Typography variant="body2">
                        Total Hours: {formatDuration(currentTimeEntry.totalHours)}
                      </Typography>
                      {currentTimeEntry.overtimeHours && (
                        <Typography variant="body2" color="orange">
                          Overtime: {formatDuration(currentTimeEntry.overtimeHours)}
                        </Typography>
                      )}
                    </>
                  )}
                  
                  {currentTimeEntry.notes && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      Notes: {currentTimeEntry.notes}
                    </Typography>
                  )}
                </>
              )}
              
              {!currentTimeEntry && (
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                  No time entry for today
                </Typography>
              )}
            </Box>
          </Paper>
        </Box>

        {/* Quick Actions Card */}
        <Paper sx={{ p: 3 }} data-onboarding-target="quick-actions-card">
          <Typography variant="h6" gutterBottom>
            Quick Actions
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Button
              variant="outlined"
              onClick={() => window.location.href = '/time-entries'}
              sx={{ flex: '1 1 200px' }}
            >
              View Time Entries
            </Button>
            <Button
              variant="outlined"
              onClick={() => window.location.href = '/reports'}
              sx={{ flex: '1 1 200px' }}
            >
              View Reports
            </Button>
          </Box>
        </Paper>
      </Box>

      {/* Clock Out Dialog */}
      <Dialog open={clockOutDialog} onClose={() => setClockOutDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Clock Out</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            id="notes"
            label="Notes (optional)"
            type="text"
            fullWidth
            variant="outlined"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            id="breakTime"
            label="Break Time (HH:MM, optional)"
            type="text"
            fullWidth
            variant="outlined"
            value={breakTime}
            onChange={(e) => setBreakTime(e.target.value)}
            placeholder="00:30"
            helperText="Enter break time in HH:MM format (e.g., 00:30 for 30 minutes)"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setClockOutDialog(false)}>Cancel</Button>
          <Button onClick={handleClockOut} variant="contained" disabled={loading}>
            Clock Out
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Dashboard;