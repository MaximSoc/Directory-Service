export type Department = {
  id: string;
  name: string;
  identifier: string;
  parentId?: string | undefined;
  path: string;
  depth: number;
  isActive: boolean;
  positions: DepartmentPositions[];
  locations: DepartmentLocations[];
  children: DepartmentChildren[];
};

type DepartmentPositions = {
  id: string;
  name: string;
};

type DepartmentLocations = {
  id: string;
  name: string;
};

type DepartmentChildren = {
  id: string;
  name: string;
};
