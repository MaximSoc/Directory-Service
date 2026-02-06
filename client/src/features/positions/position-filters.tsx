import { useState } from "react";
import { useGetPositionFilter } from "./model/positions-filter-store";
import { PositionsSearch } from "./position-search";
import { PositionDepartmentsFilter } from "./positions-departments-filters";
import { PositionSortControls } from "./positions-sort-controls";
import { PositionStatusFilter } from "./positions-status-filters";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { Button } from "@/shared/components/ui/button";
import { ChevronDown, Filter } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";

export function PositionsFilters() {
  const { departmentIds, isActive } = useGetPositionFilter();
  const [openDepartments, setOpenDepartments] = useState(false);

  const activeFiltersCount =
    (departmentIds?.length || 0) + (isActive !== undefined ? 1 : 0);

  return (
    <div className="flex flex-col lg:flex-row lg:items-center gap-4 w-full">
      <div className="flex-1 min-w-0">
        <PositionsSearch />
      </div>

      <div className="flex items-center gap-2 flex-wrap">
        <PositionStatusFilter compact />

        <Dialog open={openDepartments} onOpenChange={setOpenDepartments}>
          <DialogTrigger asChild>
            <Button
              variant="outline"
              size="sm"
              className="gap-1 whitespace-nowrap"
            >
              <Filter className="h-4 w-4" />
              Департаменты
              {departmentIds?.length > 0 && (
                <Badge variant="secondary" className="ml-1 h-5 w-5 p-0 text-xs">
                  {departmentIds.length}
                </Badge>
              )}
              <ChevronDown className="h-4 w-4" />
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl max-h-[80vh] p-1">
            <DialogHeader className="pb-4">
              <DialogTitle>Фильтр по подразделениям</DialogTitle>
            </DialogHeader>
            <PositionDepartmentsFilter />
          </DialogContent>
        </Dialog>

        {activeFiltersCount > 0 && (
          <div className="flex items-center gap-1 bg-muted px-2 py-1 rounded-lg">
            <span className="text-xs text-muted-foreground">
              Активно: {activeFiltersCount}
            </span>
          </div>
        )}

        <PositionSortControls compact />
      </div>
    </div>
  );
}
