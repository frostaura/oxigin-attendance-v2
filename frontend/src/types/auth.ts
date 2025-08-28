export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  employeeId: string;
  department: string;
  jobTitle: string;
  hireDate: string;
  isActive: boolean;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  employeeId: string;
  department: string;
  jobTitle: string;
  hireDate: string;
}

export interface AuthResponse {
  token: string;
  expiration: string;
  user: User;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}