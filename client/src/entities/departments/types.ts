export type Department = {
  id: string;
  name: string;
  identifier: string;
  parentId?: string | undefined;
  path: string;
  depth: number;
  isActive: boolean;
  hasMoreChildren: boolean;
  positions: DepartmentPositions[];
  locations: DepartmentLocations[];
};

type DepartmentPositions = {
  id: string;
  name: string;
};

type DepartmentLocations = {
  id: string;
  name: string;
};
