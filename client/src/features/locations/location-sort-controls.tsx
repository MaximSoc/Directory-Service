import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  LocationSortField,
  setLocationFilterSortBy,
  setLocationFilterSortDirection,
  useGetLocationFilter,
} from "./model/locations-filter-store";
import { Button } from "@/shared/components/ui/button";
import { ArrowDownAZ, ArrowUpAZ } from "lucide-react";

export function LocationSortControls() {
  const { sortBy, sortDirection } = useGetLocationFilter();
  const SORT_OPTIONS: { value: LocationSortField; label: string }[] = [
    { value: "name", label: "Название" },
    { value: "city", label: "Город" },
    { value: "country", label: "Страна" },
    { value: "region", label: "Регион" },
    { value: "isActive", label: "Статус" },
  ];
  const toggleSortDirection = () => {
    setLocationFilterSortDirection(sortDirection === "asc" ? "desc" : "asc");
  };

  return (
    <div className="flex items-center space-x-2">
      <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
        Сортировка:
      </span>
      <Select
        value={sortBy}
        onValueChange={(value) =>
          setLocationFilterSortBy(value as LocationSortField)
        }
      >
        <SelectTrigger className="w-32.5">
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
        size="icon"
        onClick={toggleSortDirection}
        title={sortDirection === "asc" ? "По возрастанию" : "По убыванию"}
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
