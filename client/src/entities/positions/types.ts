export type Position = {
  id: string;
  name: string;
  description: string | undefined;
  isActive: boolean;
  departmentNames: string[];
  departmentCount: number;
};
