"use client";

import { DepartmentsList } from "@/features/departments/ui/departments-list";
import { DepartmentsToolbar } from "@/features/departments/ui/departments-toolbar";
import { useState } from "react";

export type ViewMode = "list" | "tree";

export default function DepartmentsPage() {
  const [viewMode, setViewMode] = useState<ViewMode>("list");

  return (
    <div className="container mx-auto py-8 px-4 sm:px-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">Подразделения</h1>
      </div>

      <DepartmentsToolbar viewMode={viewMode} onViewModeChange={setViewMode} />
      <DepartmentsList viewMode={viewMode} />
    </div>
  );
}
