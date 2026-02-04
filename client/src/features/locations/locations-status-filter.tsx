import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  setLocationFilterIsActive,
  useGetLocationFilter,
} from "./model/locations-filter-store";

export function LocationStatusFilter() {
  const { isActive } = useGetLocationFilter();

  return (
    <div className="flex items-center space-x-2">
      <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
        Статус:
      </span>
      <Select
        value={isActive === undefined ? "all" : isActive ? "true" : "false"}
        onValueChange={(value) => {
          if (value === "all") setLocationFilterIsActive(undefined);
          else if (value === "false") setLocationFilterIsActive(false);
          else setLocationFilterIsActive(true);
        }}
      >
        <SelectTrigger className="w-32.5">
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
