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
  PositionSortField,
  setPositionFilterSortBy,
  setPositionFilterSortDirection,
  useGetPositionFilter,
} from "./model/positions-filter-store";

interface PositionSortControlsProps {
  compact?: boolean;
}

export function PositionSortControls({
  compact = false,
}: PositionSortControlsProps) {
  const { sortBy, sortDirection } = useGetPositionFilter();
  const SORT_OPTIONS: { value: PositionSortField; label: string }[] = [
    { value: "name", label: "Название" },
    { value: "isActive", label: "Статус" },
  ];
  const toggleSortDirection = () => {
    setPositionFilterSortDirection(sortDirection === "asc" ? "desc" : "asc");
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
          setPositionFilterSortBy(value as PositionSortField)
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
