"use client";

import { useDepartmentsViewStore } from "@/features/departments/model/use-departments-view-store";
import { DepartmentsToolbar } from "@/features/departments/ui/departments-toolbar";
import { DepartmentsView } from "@/features/departments/ui/departments-view";

export default function DepartmentsPage() {
  const { viewMode, setViewMode } = useDepartmentsViewStore();

  return (
    <div className="container mx-auto py-8 px-4 sm:px-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">Подразделения</h1>
      </div>

      <DepartmentsToolbar viewMode={viewMode} onViewModeChange={setViewMode} />
      <DepartmentsView viewMode={viewMode} />
    </div>
  );
}
