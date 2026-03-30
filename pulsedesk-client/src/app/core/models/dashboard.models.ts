// src/app/core/models/dashboard.models.ts
export interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  newUsersToday: number;
  newUsersThisWeek: number;
}

export interface UserRegisteredEvent {
  id: number;
  fullName: string;
  email: string;
  role: string;
  department?: string;
  registeredAt: string;
}

export interface UserListItem {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  role: string;
  department?: string;
  jobTitle?: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}
