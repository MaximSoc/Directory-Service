import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  setDepartmentsFilterIsActive,
  useGetDepartmentsFilter,
} from "./model/departments-filter-store";

interface DepartmentStatusFilterProps {
  compact?: boolean;
}

export function DepartmentStatusFilter({
  compact = false,
}: DepartmentStatusFilterProps) {
  const { isActive } = useGetDepartmentsFilter();

  const getValue = () => {
    if (isActive === undefined) return "all";
    return isActive ? "true" : "false";
  };

  return (
    <div className="flex items-center space-x-2">
      {!compact && (
        <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
          Статус:
        </span>
      )}
      <Select
        value={getValue()}
        onValueChange={(value) => {
          if (value === "all") setDepartmentsFilterIsActive(undefined);
          else if (value === "false") setDepartmentsFilterIsActive(false);
          else setDepartmentsFilterIsActive(true);
        }}
      >
        <SelectTrigger className={compact ? "w-28 h-8 text-xs" : "w-32.5"}>
          <SelectValue placeholder="Статус" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Все</SelectItem>
          <SelectItem value="true">Активные</SelectItem>
          <SelectItem value="false">Неактивные</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}
