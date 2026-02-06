import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  setPositionFilterIsActive,
  useGetPositionFilter,
} from "./model/positions-filter-store";

interface PositionStatusFilterProps {
  compact?: boolean;
}

export function PositionStatusFilter({
  compact = false,
}: PositionStatusFilterProps) {
  const { isActive } = useGetPositionFilter();

  return (
    <div className="flex items-center space-x-2">
      {!compact && (
        <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
          Статус:
        </span>
      )}
      <Select
        value={isActive === undefined ? "all" : isActive ? "true" : "false"}
        onValueChange={(value) => {
          if (value === "all") setPositionFilterIsActive(undefined);
          else if (value === "false") setPositionFilterIsActive(false);
          else setPositionFilterIsActive(true);
        }}
      >
        <SelectTrigger className={compact ? "w-28 h-8 text-xs" : "w-32.5"}>
          <SelectValue />
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
