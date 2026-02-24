"use client";

import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { Plus, List, Network } from "lucide-react"; // Иконки для режимов
import { DepartmentsFilters } from "../departments-filters";
import { CreateDepartmentDialog } from "../create-department-dialog";
import { Tabs, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";
import { ViewMode } from "../model/use-departments-view-store";

interface ToolbarProps {
  viewMode: ViewMode;
  onViewModeChange: (mode: ViewMode) => void;
}

export function DepartmentsToolbar({
  viewMode,
  onViewModeChange,
}: ToolbarProps) {
  const [openCreate, setOpenCreate] = useState(false);

  return (
    <div className="mb-8 flex flex-col gap-4">
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 w-full">
        <div className="flex items-center gap-4 flex-wrap">
          <Tabs
            value={viewMode}
            onValueChange={(v) => onViewModeChange(v as ViewMode)}
            className="w-auto"
          >
            <TabsList>
              <TabsTrigger value="list" className="gap-2">
                <List className="h-4 w-4" />
                <span className="hidden sm:inline">Список</span>
              </TabsTrigger>
              <TabsTrigger value="tree" className="gap-2">
                <Network className="h-4 w-4" />
                <span className="hidden sm:inline">Дерево</span>
              </TabsTrigger>
            </TabsList>
          </Tabs>

          <DepartmentsFilters />
        </div>

        <div className="flex items-center gap-2">
          <Button variant="default" onClick={() => setOpenCreate(true)}>
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
