export type Department = {
  id: string;
  name: string;
  identifier: string;
  parentId: string | undefined;
  path: string;
  depth: number;
  isActive: boolean;
};

export type GetDepartmentsResponse = {
  departments: Department[];
};

export type GetDepartmentsRequest = {
  search?: string;
  isActive?: boolean;
};
