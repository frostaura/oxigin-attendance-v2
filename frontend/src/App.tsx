import React, { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Provider } from 'react-redux';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';

import { store } from './store';
import { useAppDispatch } from './utils/hooks';
import { initializeAuth } from './store/authSlice';
import { ToastProvider, ThemeProvider } from './ui';

// Components
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import Dashboard from './components/dashboard/Dashboard';
import ComponentShowcase from './components/common/ComponentShowcase';
import Layout from './components/layout/Layout';
import ProtectedRoute from './components/common/ProtectedRoute';

const AppContent: React.FC = () => {
  const dispatch = useAppDispatch();

  useEffect(() => {
    dispatch(initializeAuth());
  }, [dispatch]);

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Layout>
                <Dashboard />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/showcase"
          element={
            <ProtectedRoute>
              <Layout>
                <ComponentShowcase />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </Router>
  );
};

const App: React.FC = () => {
  return (
    <Provider store={store}>
      <ThemeProvider>
        <ToastProvider>
          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
              <AppContent />
            </div>
          </LocalizationProvider>
        </ToastProvider>
      </ThemeProvider>
    </Provider>
  );
};

export default App;
