export type Location = {
  id: string;
  name: string;
  country: string;
  region: string;
  city: string;
  postalCode: string;
  street: string;
  apartamentNumber: string;
  timezone: string;
  isActive: boolean;
};

export type GetLocationsByDepartmentResponse = {
  locations: Location[];
  totalPages: number;
  page: number;
};

export type GetLocationsByDepartmentRequest = {
  search?: string;
  page: number;
  pageSize: number;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
};

export type CreateLocationRequest = {
  name: string;
  country: string;
  region: string;
  city: string;
  postalCode: string;
  street: string;
  apartamentNumber: string;
  timezone: string;
};

export type UpdateLocationRequest = {
  name: string;
  country: string;
  region: string;
  city: string;
  postalCode: string;
  street: string;
  apartamentNumber: string;
  timezone: string;
};
