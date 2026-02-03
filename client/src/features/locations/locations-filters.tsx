import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  setLocationFilterIsActive,
  setLocationFilterSearch,
  setLocationFilterSortBy,
  setLocationFilterSortDirection,
  SortField,
  useGetLocationFilter,
} from "./model/locations-filter-store";
import { useDebounce } from "use-debounce";
import { useEffect, useState } from "react";
import { ArrowDownAZ, ArrowUpAZ } from "lucide-react";
import { Button } from "@/shared/components/ui/button";

const SORT_OPTIONS: { value: SortField; label: string }[] = [
  { value: "name", label: "Название" },
  { value: "city", label: "Город" },
  { value: "country", label: "Страна" },
  { value: "region", label: "Регион" },
  { value: "isActive", label: "Статус" },
];

export function LocationsFilters() {
  const { search, isActive, sortBy, sortDirection } = useGetLocationFilter();

  const [localSearch, setLocalSearch] = useState(search ?? "");
  const [debouncedSearch] = useDebounce(localSearch, 300);

  useEffect(() => {
    setLocationFilterSearch(debouncedSearch);
  }, [debouncedSearch]);

  const toggleSortDirection = () => {
    setLocationFilterSortDirection(sortDirection === "asc" ? "desc" : "asc");
  };

  return (
    <div className="flex w-full flex-col xl:flex-row items-start xl:items-center gap-4">
      <div className="flex w-full max-w-md items-center space-x-2">
        <Input
          placeholder="Поиск по названию..."
          value={localSearch}
          onChange={(e) => setLocalSearch(e.target.value)}
        />
      </div>

      <div className="flex flex-wrap items-center gap-2">
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

        <div className="flex items-center space-x-2 border-l pl-2 ml-2 border-border/50">
          <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
            Сортировка:
          </span>
          <Select
            value={sortBy}
            onValueChange={(value) =>
              setLocationFilterSortBy(value as SortField)
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
      </div>
    </div>
  );
}
