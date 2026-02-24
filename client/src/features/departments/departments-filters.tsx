import { DepartmentsSearch } from "./department-search";
import { DepartmentStatusFilter } from "./departments-status-filters";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { Button } from "@/shared/components/ui/button";
import { ChevronDown, Filter } from "lucide-react";
import { ParentDepartmentFilter } from "./parent-department-filter";
import { useGetDepartmentsFilter } from "./model/departments-filter-store";
import { useState } from "react";
import { Badge } from "@/shared/components/ui/badge";
import { DepartmentSortControls } from "./departments-sort-controls";
import { DepartmentLocationFilter } from "./department-locations-filter";

export function DepartmentsFilters() {
  const { parentId, isActive, locationIds } = useGetDepartmentsFilter();
  const [openParentFilter, setOpenParentFilter] = useState(false);

  const activeFiltersCount =
    (parentId ? 1 : 0) +
    (isActive !== undefined ? 1 : 0) +
    (locationIds.length > 0 ? 1 : 0);

  return (
    <div className="flex flex-col lg:flex-row lg:items-center gap-4 w-full">
      <div className="flex-1 min-w-0">
        <DepartmentsSearch />
      </div>

      <div className="flex items-center gap-2 flex-wrap">
        <DepartmentStatusFilter compact />

        <DepartmentLocationFilter />

        <Dialog open={openParentFilter} onOpenChange={setOpenParentFilter}>
          <DialogTrigger asChild>
            <Button
              variant="outline"
              size="sm"
              className="gap-1 whitespace-nowrap h-9"
            >
              <Filter className="h-4 w-4" />
              Родитель
              {parentId && (
                <Badge
                  variant="secondary"
                  className="ml-1 px-1.5 h-5 min-w-5 flex items-center justify-center text-[10px]"
                >
                  1
                </Badge>
              )}
              <ChevronDown className="h-4 w-4 opacity-50" />
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md max-h-[85vh] p-6">
            <DialogHeader className="pb-4">
              <DialogTitle>Родительское подразделение</DialogTitle>
              <DialogDescription>
                Выберите подразделение, чтобы увидеть только его дочерние
                элементы.
              </DialogDescription>
            </DialogHeader>
            <ParentDepartmentFilter />
          </DialogContent>
        </Dialog>

        <DepartmentSortControls compact />

        {activeFiltersCount > 0 && (
          <div className="flex items-center gap-1 bg-muted px-2 py-1 rounded-lg border border-border shadow-sm">
            <span className="text-[10px] font-medium uppercase text-muted-foreground">
              Активно: {activeFiltersCount}
            </span>
          </div>
        )}
      </div>
    </div>
  );
}
