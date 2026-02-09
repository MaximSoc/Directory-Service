export type Position = {
  id: string;
  name: string;
  description: string | undefined;
  isActive: boolean;
  departmentNames: string[];
  departmentIds: string[];
  departmentCount: number;
};

export type GetPositionsResponse = {
  positions: Position[];
  totalPages: number;
  page: number;
};

export type GetPositionsRequest = {
  search?: string;
  page: number;
  pageSize: number;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
};

export type CreatePositionRequest = {
  name: string;
  description?: string | undefined;
  departmentsIds: string[];
};

export type GetOnePositionResponse = {
  position: Position;
};

export type UpdatePositionRequest = {
  name: string;
  description?: string | undefined;
  departmentsIds: string[];
};
