import React, { useState, useEffect } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { loginStart, loginSuccess, loginFailure, clearError } from '../../store/authSlice';
import { authApi } from '../../services/api';
import { LoginRequest } from '../../types/auth';
import { Button, Input, Typography, Alert } from '../../ui';

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
    <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div className="bg-white rounded-2xl shadow-lg p-8">
          <div className="text-center">
            <Typography variant="h2" className="mb-2">
              Oxigin Attendance
            </Typography>
            
            <Typography variant="h4" className="mb-8 text-gray-600">
              Sign In
            </Typography>
          </div>

          {error && (
            <Alert variant="error" className="mb-6">
              {error}
            </Alert>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <Input
              label="Email Address"
              name="email"
              type="email"
              required
              value={formData.email}
              onChange={handleChange}
              disabled={loading}
              placeholder="Enter your email"
            />
            
            <Input
              label="Password"
              name="password"
              type="password"
              required
              value={formData.password}
              onChange={handleChange}
              disabled={loading}
              placeholder="Enter your password"
            />
            
            <Button
              type="submit"
              className="w-full"
              size="lg"
              disabled={loading}
            >
              {loading ? 'Signing In...' : 'Sign In'}
            </Button>

            {/* Quick Access Demo Buttons */}
            <div className="mt-6">
              <div className="relative">
                <div className="absolute inset-0 flex items-center">
                  <div className="w-full border-t border-gray-300" />
                </div>
                <div className="relative flex justify-center text-sm">
                  <span className="px-2 bg-white text-gray-500">Quick Demo Access</span>
                </div>
              </div>
              
              <div className="mt-6 space-y-3">
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full text-primary-600 border-primary-200"
                  onClick={() => handleQuickLogin('admin@oxigin.com', 'Admin@123')}
                  disabled={loading}
                >
                  Administrator Demo (admin@oxigin.com)
                </Button>
                
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full text-purple-600 border-purple-200"
                  onClick={() => handleQuickLogin('manager@oxigin.com', 'Manager@123')}
                  disabled={loading}
                >
                  Manager Demo (manager@oxigin.com)
                </Button>
                
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full text-success-600 border-success-200"
                  onClick={() => handleQuickLogin('employee@oxigin.com', 'Employee@123')}
                  disabled={loading}
                >
                  Employee Demo (employee@oxigin.com)
                </Button>
              </div>
            </div>

            <div className="text-center">
              <Typography variant="body2" color="textSecondary">
                Don't have an account?{' '}
                <RouterLink 
                  to="/register" 
                  className="text-primary-600 hover:text-primary-700 font-medium"
                >
                  Sign up
                </RouterLink>
              </Typography>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Login;