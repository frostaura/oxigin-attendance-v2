import React, { useEffect, useState } from 'react';
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
import { 
  Button, 
  Alert, 
  Badge, 
  Card, 
  CardHeader, 
  CardTitle, 
  CardContent, 
  Typography,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Input,
  PlayIcon,
  StopIcon,
  ClockIcon,
  CalendarIcon,
  useToast,
  SkeletonDashboard,
} from '../../ui';

const Dashboard: React.FC = () => {
  const [clockOutDialog, setClockOutDialog] = useState(false);
  const [notes, setNotes] = useState('');
  const [breakTime, setBreakTime] = useState('');
  const [currentTime, setCurrentTime] = useState(new Date());
  
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { currentTimeEntry, loading, error } = useAppSelector((state) => state.timeEntry);
  const { addToast } = useToast();

  // Update current time every second
  useEffect(() => {
    const timer = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);
    return () => clearInterval(timer);
  }, []);

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
      
      // Show success toast
      addToast({
        variant: 'success',
        title: 'Clocked In',
        description: `Successfully clocked in at ${formatTime(new Date())}`,
      });
    } catch (error: any) {
      const message = error.response?.data?.message || 'Failed to clock in';
      dispatch(setError(message));
      addToast({
        variant: 'error',
        title: 'Clock In Failed',
        description: message,
      });
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
      
      // Show success toast
      addToast({
        variant: 'success',
        title: 'Clocked Out',
        description: `Successfully clocked out at ${formatTime(new Date())}`,
      });
    } catch (error: any) {
      const message = error.response?.data?.message || 'Failed to clock out';
      dispatch(setError(message));
      addToast({
        variant: 'error',
        title: 'Clock Out Failed',
        description: message,
      });
    } finally {
      dispatch(setLoading(false));
    }
  };

  const isCurrentlyClockedIn = currentTimeEntry?.status === TimeEntryStatus.Active;

  // Show skeleton loading while loading
  if (loading && !currentTimeEntry) {
    return <SkeletonDashboard />;
  }

  return (
    <div className="space-y-6">
      <Typography variant="h3" className="text-gray-900 dark:text-white">
        Welcome back, {user?.firstName}!
      </Typography>
      
      {error && (
        <Alert variant="error" onClose={() => dispatch(clearError())}>
          {error}
        </Alert>
      )}

      <div className="space-y-6">
        {/* Clock In/Out Card */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <Card data-onboarding-target="time-tracking-card">
            <CardHeader className="text-center">
              <CardTitle className="flex items-center justify-center gap-2">
                <ClockIcon className="h-5 w-5" />
                Time Tracking
              </CardTitle>
            </CardHeader>
            <CardContent className="text-center space-y-4">
              {isCurrentlyClockedIn ? (
                <>
                  <Badge 
                    label="Currently Clocked In" 
                    variant="success"
                    size="md"
                    className="mb-4"
                  />
                  <div className="space-y-2">
                    <Typography variant="body1">
                      Clock In Time: {formatTime(currentTimeEntry.clockInTime)}
                    </Typography>
                    <Typography variant="body1">
                      Current Time: {formatTime(currentTime)}
                    </Typography>
                  </div>
                  <Button
                    variant="destructive"
                    size="lg"
                    onClick={handleClockOutClick}
                    disabled={loading}
                    startIcon={<StopIcon />}
                    className="mt-4"
                  >
                    Clock Out
                  </Button>
                </>
              ) : (
                <>
                  <Badge 
                    label="Not Clocked In" 
                    variant="secondary"
                    size="md"
                    className="mb-4"
                  />
                  <Typography variant="body1" className="mb-4">
                    Current Time: {formatTime(currentTime)}
                  </Typography>
                  <Button
                    variant="primary"
                    size="lg"
                    onClick={handleClockIn}
                    disabled={loading}
                    startIcon={<PlayIcon />}
                  >
                    Clock In
                  </Button>
                </>
              )}
            </CardContent>
          </Card>

          {/* Today's Summary Card */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <CalendarIcon className="h-5 w-5" />
                Today's Summary
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <Typography variant="body2" color="textSecondary">
                Date: {getTodayDateString()}
              </Typography>
              
              {currentTimeEntry && (
                <div className="space-y-2">
                  <Typography variant="body2">
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
                        <Typography variant="body2" className="text-warning-600">
                          Overtime: {formatDuration(currentTimeEntry.overtimeHours)}
                        </Typography>
                      )}
                    </>
                  )}
                  
                  {currentTimeEntry.notes && (
                    <Typography variant="body2">
                      Notes: {currentTimeEntry.notes}
                    </Typography>
                  )}
                </div>
              )}
              
              {!currentTimeEntry && (
                <Typography variant="body2" color="textSecondary">
                  No time entry for today
                </Typography>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Quick Actions Card */}
        <Card data-onboarding-target="quick-actions-card">
          <CardHeader>
            <CardTitle>Quick Actions</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex gap-4 flex-wrap">
              <Button
                variant="outline"
                onClick={() => window.location.href = '/time-entries'}
                className="flex-1 min-w-[200px]"
              >
                View Time Entries
              </Button>
              <Button
                variant="outline"
                onClick={() => window.location.href = '/reports'}
                className="flex-1 min-w-[200px]"
              >
                View Reports
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Clock Out Modal */}
      <Modal 
        isOpen={clockOutDialog} 
        onClose={() => setClockOutDialog(false)}
        size="md"
      >
        <ModalHeader>Clock Out</ModalHeader>
        <ModalBody className="space-y-4">
          <Input
            label="Notes (optional)"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Add any notes about your work session..."
          />
          <Input
            label="Break Time (HH:MM, optional)"
            value={breakTime}
            onChange={(e) => setBreakTime(e.target.value)}
            placeholder="00:30"
            // helperText="Enter break time in HH:MM format (e.g., 00:30 for 30 minutes)"
          />
          <Typography variant="caption" color="textSecondary">
            Enter break time in HH:MM format (e.g., 00:30 for 30 minutes)
          </Typography>
        </ModalBody>
        <ModalFooter>
          <Button 
            variant="outline" 
            onClick={() => setClockOutDialog(false)}
          >
            Cancel
          </Button>
          <Button 
            variant="primary" 
            onClick={handleClockOut} 
            disabled={loading}
          >
            Clock Out
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};

export default Dashboard;