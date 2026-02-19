"use client";

import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { Plus } from "lucide-react";
import { DepartmentsFilters } from "../departments-filters";
import { CreateDepartmentDialog } from "../create-department-dialog";

export function DepartmentsToolbar() {
  const [openCreate, setOpenCreate] = useState(false);

  return (
    <div className="mb-8 flex flex-col gap-4">
      <div className="flex flex-col lg:flex-row lg:items-center gap-4 w-full">
        <div className="flex items-center gap-2 flex-wrap">
          <DepartmentsFilters />

          <Button
            variant="default"
            onClick={() => setOpenCreate(true)}
            className="ml-auto lg:ml-0"
          >
            <Plus className="h-4 w-4 mr-2" /> Создать подразделение
          </Button>
        </div>
        <CreateDepartmentDialog
          open={openCreate}
          onOpenChange={setOpenCreate}
        />
      </div>
    </div>
  );
}
