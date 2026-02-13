export type Position = {
  id: string;
  name: string;
  description: string | undefined;
  isActive: boolean;
  departments: PositionDepartment[];
  departmentCount: number;
};

export type PositionDepartment = {
  id: string;
  name: string;
};
