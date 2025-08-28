import React, { useState, useEffect } from 'react';
import {
  Box,
  TextField,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import dayjs, { Dayjs } from 'dayjs';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { loginStart, loginSuccess, loginFailure, clearError } from '../../store/authSlice';
import { authApi } from '../../services/api';
import { RegisterRequest } from '../../types/auth';
import { Button, Typography, Alert } from '../../ui';

const Register: React.FC = () => {
  const [formData, setFormData] = useState<RegisterRequest>({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    employeeId: '',
    department: '',
    jobTitle: '',
    hireDate: dayjs().format('YYYY-MM-DD'),
  });
  const [hireDate, setHireDate] = useState<Dayjs | null>(dayjs());

  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { loading, error, isAuthenticated } = useAppSelector((state) => state.auth);

  useEffect(() => {
    if (isAuthenticated) {
      navigate('/dashboard');
    }
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    dispatch(clearError());
  }, [dispatch]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleDateChange = (date: Dayjs | null) => {
    setHireDate(date);
    setFormData({
      ...formData,
      hireDate: date ? date.format('YYYY-MM-DD') : dayjs().format('YYYY-MM-DD'),
    });
  };

  const validateForm = (): string | null => {
    if (!formData.email || !formData.password || !formData.firstName || 
        !formData.lastName || !formData.employeeId) {
      return 'Please fill in all required fields';
    }

    if (formData.password.length < 6) {
      return 'Password must be at least 6 characters long';
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      return 'Please enter a valid email address';
    }

    return null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const validationError = validateForm();
    if (validationError) {
      dispatch(loginFailure(validationError));
      return;
    }

    try {
      dispatch(loginStart());
      const response = await authApi.register(formData);
      dispatch(loginSuccess({
        user: response.user,
        token: response.token,
      }));
      navigate('/dashboard');
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Registration failed. Please try again.';
      dispatch(loginFailure(errorMessage));
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-2xl w-full space-y-8">
        <div className="bg-white rounded-2xl shadow-lg p-8">
          <div className="text-center">
            <Typography variant="h2" className="mb-2">
              Oxigin Attendance
            </Typography>
            
            <Typography variant="h4" className="mb-8 text-gray-600">
              Create Account
            </Typography>
          </div>

          {error && (
            <Alert variant="error" className="mb-6">
              {error}
            </Alert>
          )}

          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <TextField
                    required
                    fullWidth
                    id="firstName"
                    label="First Name"
                    name="firstName"
                    autoComplete="given-name"
                    value={formData.firstName}
                    onChange={handleChange}
                    disabled={loading}
                  />
                  <TextField
                    required
                    fullWidth
                    id="lastName"
                    label="Last Name"
                    name="lastName"
                    autoComplete="family-name"
                    value={formData.lastName}
                    onChange={handleChange}
                    disabled={loading}
                  />
                </Box>
                <TextField
                  required
                  fullWidth
                  id="email"
                  label="Email Address"
                  name="email"
                  autoComplete="email"
                  value={formData.email}
                  onChange={handleChange}
                  disabled={loading}
                />
                <TextField
                  required
                  fullWidth
                  name="password"
                  label="Password"
                  type="password"
                  id="password"
                  autoComplete="new-password"
                  value={formData.password}
                  onChange={handleChange}
                  disabled={loading}
                  helperText="Password must be at least 6 characters long"
                />
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <TextField
                    required
                    fullWidth
                    id="employeeId"
                    label="Employee ID"
                    name="employeeId"
                    value={formData.employeeId}
                    onChange={handleChange}
                    disabled={loading}
                  />
                  <TextField
                    fullWidth
                    id="department"
                    label="Department"
                    name="department"
                    value={formData.department}
                    onChange={handleChange}
                    disabled={loading}
                  />
                </Box>
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <TextField
                    fullWidth
                    id="jobTitle"
                    label="Job Title"
                    name="jobTitle"
                    value={formData.jobTitle}
                    onChange={handleChange}
                    disabled={loading}
                  />
                  <DatePicker
                    label="Hire Date"
                    value={hireDate}
                    onChange={handleDateChange}
                    disabled={loading}
                    slotProps={{
                      textField: {
                        fullWidth: true,
                      },
                    }}
                  />
                </Box>
              </Box>
              <Button
                type="submit"
                className="w-full mt-6 mb-4"
                size="lg"
                disabled={loading}
              >
                {loading ? 'Creating Account...' : 'Sign Up'}
              </Button>
              
              <div className="text-center">
                <Typography variant="body2" color="textSecondary">
                  Already have an account?{' '}
                  <RouterLink 
                    to="/login" 
                    className="text-primary-600 hover:text-primary-700 font-medium"
                  >
                    Sign In
                  </RouterLink>
                </Typography>
              </div>
            </Box>
          </LocalizationProvider>
        </div>
      </div>
    </div>
  );
};

export default Register;