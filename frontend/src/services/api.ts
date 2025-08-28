import axios, { AxiosResponse } from 'axios';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../types/auth';
import { TimeEntry, ClockInRequest, ClockOutRequest, TimeReport, TimeEntryFilters } from '../types/timeEntry';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7017/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle auth errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth API
export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response: AxiosResponse<AuthResponse> = await api.post('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response: AxiosResponse<AuthResponse> = await api.post('/auth/register', data);
    return response.data;
  },

  getCurrentUser: async (): Promise<User> => {
    const response: AxiosResponse<User> = await api.get('/auth/me');
    return response.data;
  },

  validateToken: async (): Promise<{ message: string; userId: string }> => {
    const response = await api.get('/auth/validate');
    return response.data;
  },
};

// TimeEntry API
export const timeEntryApi = {
  clockIn: async (data: ClockInRequest): Promise<TimeEntry> => {
    const response: AxiosResponse<TimeEntry> = await api.post('/timeentry/clock-in', data);
    return response.data;
  },

  clockOut: async (data: ClockOutRequest): Promise<TimeEntry> => {
    const response: AxiosResponse<TimeEntry> = await api.post('/timeentry/clock-out', data);
    return response.data;
  },

  getActiveTimeEntry: async (): Promise<TimeEntry | null> => {
    try {
      const response: AxiosResponse<TimeEntry> = await api.get('/timeentry/active');
      return response.data;
    } catch (error) {
      return null;
    }
  },

  getTimeEntries: async (filters?: TimeEntryFilters): Promise<TimeEntry[]> => {
    const params = new URLSearchParams();
    if (filters?.startDate) params.append('startDate', filters.startDate);
    if (filters?.endDate) params.append('endDate', filters.endDate);
    
    const response: AxiosResponse<TimeEntry[]> = await api.get(`/timeentry?${params}`);
    return response.data;
  },

  getTimeReport: async (startDate: string, endDate: string): Promise<TimeReport> => {
    const response: AxiosResponse<TimeReport> = await api.get(
      `/timeentry/report?startDate=${startDate}&endDate=${endDate}`
    );
    return response.data;
  },

  getAllEmployeesTimeReport: async (startDate: string, endDate: string): Promise<TimeReport[]> => {
    const response: AxiosResponse<TimeReport[]> = await api.get(
      `/timeentry/report/all?startDate=${startDate}&endDate=${endDate}`
    );
    return response.data;
  },
};

export default api;