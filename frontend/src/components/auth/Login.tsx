import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  TextField,
  Typography,
  Paper,
  Container,
  Alert,
  CircularProgress,
  Link,
  Divider,
  Chip,
} from '@mui/material';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { loginStart, loginSuccess, loginFailure, clearError } from '../../store/authSlice';
import { authApi } from '../../services/api';
import { LoginRequest } from '../../types/auth';

const Login: React.FC = () => {
  const [formData, setFormData] = useState<LoginRequest>({
    email: '',
    password: '',
  });

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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.email || !formData.password) {
      dispatch(loginFailure('Please fill in all fields'));
      return;
    }

    try {
      dispatch(loginStart());
      const response = await authApi.login(formData);
      dispatch(loginSuccess({
        user: response.user,
        token: response.token,
      }));
      navigate('/dashboard');
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Login failed. Please try again.';
      dispatch(loginFailure(errorMessage));
    }
  };

  const handleQuickLogin = (email: string, password: string) => {
    setFormData({ email, password });
  };

  return (
    <Container component="main" maxWidth="sm">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Paper
          elevation={3}
          sx={{
            padding: 4,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            width: '100%',
          }}
        >
          <Typography component="h1" variant="h4" sx={{ mb: 3 }}>
            Oxigin Attendance
          </Typography>
          
          <Typography component="h2" variant="h5" sx={{ mb: 3 }}>
            Sign In
          </Typography>

          {error && (
            <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email Address"
              name="email"
              autoComplete="email"
              autoFocus
              value={formData.email}
              onChange={handleChange}
              disabled={loading}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Password"
              type="password"
              id="password"
              autoComplete="current-password"
              value={formData.password}
              onChange={handleChange}
              disabled={loading}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={loading}
              startIcon={loading && <CircularProgress size={20} />}
            >
              {loading ? 'Signing In...' : 'Sign In'}
            </Button>

            {/* Quick Access Demo Buttons */}
            <Box sx={{ mt: 2, mb: 2 }}>
              <Divider sx={{ mb: 2 }}>
                <Chip label="Quick Demo Access" size="small" />
              </Divider>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                <Button
                  variant="outlined"
                  size="small"
                  onClick={() => handleQuickLogin('admin@oxigin.com', 'Admin@123')}
                  disabled={loading}
                  sx={{ 
                    textTransform: 'none',
                    fontSize: '0.875rem',
                    color: 'primary.main',
                    borderColor: 'primary.main'
                  }}
                >
                  Administrator Demo (admin@oxigin.com)
                </Button>
                <Button
                  variant="outlined"
                  size="small"
                  onClick={() => handleQuickLogin('manager@oxigin.com', 'Manager@123')}
                  disabled={loading}
                  sx={{ 
                    textTransform: 'none',
                    fontSize: '0.875rem',
                    color: 'secondary.main',
                    borderColor: 'secondary.main'
                  }}
                >
                  Manager Demo (manager@oxigin.com)
                </Button>
                <Button
                  variant="outlined"
                  size="small"
                  onClick={() => handleQuickLogin('employee@oxigin.com', 'Employee@123')}
                  disabled={loading}
                  sx={{ 
                    textTransform: 'none',
                    fontSize: '0.875rem',
                    color: 'success.main',
                    borderColor: 'success.main'
                  }}
                >
                  Employee Demo (employee@oxigin.com)
                </Button>
              </Box>
            </Box>

            <Box textAlign="center">
              <Link component={RouterLink} to="/register" variant="body2">
                Don't have an account? Sign Up
              </Link>
            </Box>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default Login;