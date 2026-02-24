import { DepartmentsList } from "./departments-list";
import { DepartmentsTree } from "./departments-tree";

interface DepartmentsViewProps {
  viewMode: "list" | "tree";
}

export function DepartmentsView({ viewMode }: DepartmentsViewProps) {
  if (viewMode === "tree") {
    return <DepartmentsTree isActive={true} />;
  }

  return <DepartmentsList />;
}
