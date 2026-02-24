"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Button } from "@/shared/components/ui/button";
import { ArrowDownAZ, ArrowUpAZ } from "lucide-react";
import {
  DepartmentSortField,
  setDepartmentsFilterSortBy,
  setDepartmentsFilterSortDirection,
  useGetDepartmentsFilter,
} from "./model/departments-filter-store";

interface DepartmentSortControlsProps {
  compact?: boolean;
}

export function DepartmentSortControls({
  compact = false,
}: DepartmentSortControlsProps) {
  const { sortBy, sortDirection } = useGetDepartmentsFilter();

  const SORT_OPTIONS: { value: DepartmentSortField; label: string }[] = [
    { value: "name", label: "Название" },
    { value: "path", label: "Путь (структура)" },
    { value: "createdAt", label: "Дата создания" },
  ];

  const toggleSortDirection = () => {
    setDepartmentsFilterSortDirection(sortDirection === "asc" ? "desc" : "asc");
  };

  return (
    <div className="flex items-center space-x-1">
      {!compact && (
        <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
          Сортировка:
        </span>
      )}
      <Select
        value={sortBy}
        onValueChange={(value) =>
          setDepartmentsFilterSortBy(value as DepartmentSortField)
        }
      >
        <SelectTrigger className={compact ? "w-24 h-8 text-xs" : "w-32.5"}>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {SORT_OPTIONS.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <Button
        variant="outline"
        size={compact ? "sm" : "icon"}
        onClick={toggleSortDirection}
        title={sortDirection === "asc" ? "По возрастанию" : "По убыванию"}
        className={compact ? "h-8 w-8 p-0" : ""}
      >
        {sortDirection === "asc" ? (
          <ArrowDownAZ className="h-4 w-4" />
        ) : (
          <ArrowUpAZ className="h-4 w-4" />
        )}
      </Button>
    </div>
  );
}
