// src/app/core/models/api.models.ts
export interface ApiResponse<T> {
  success: boolean;
  message: string | null;
  data: T;
  errors: string[];
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}
