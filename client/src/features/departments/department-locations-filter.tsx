"use client";

import { Check, MapPin, ChevronDown, X, Search } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/shared/components/ui/popover";
import { Badge } from "@/shared/components/ui/badge";
import { cn } from "@/shared/lib/utils";
import {
  setDepartmentsFilterByLocationIds,
  useDepartmentsFilterStore,
} from "./model/departments-filter-store";
import { useCallback, useMemo, useState } from "react";
import { Input } from "@/shared/components/ui/input";
import { ScrollArea } from "@/shared/components/ui/scroll-area";
import { Spinner } from "@/shared/components/ui/spinner";
import { useQueryLocationsList } from "../locations/model/use-query-locations-list";

export function DepartmentLocationFilter() {
  const [searchTerm, setSearchTerm] = useState("");
  const { locationIds } = useDepartmentsFilterStore();

  const { data: locations = [], isLoading } = useQueryLocationsList({
    search: "",
    isActive: true,
  });

  const filteredLocations = useMemo(() => {
    if (!searchTerm.trim()) return locations;
    const lowerSearch = searchTerm.toLowerCase();
    return locations.filter((loc) =>
      loc.name.toLowerCase().includes(lowerSearch)
    );
  }, [locations, searchTerm]);

  const handleChange = useCallback(
    (id: string, checked: boolean) => {
      const newIds = checked
        ? [...locationIds, id]
        : locationIds.filter((d) => d !== id);
      setDepartmentsFilterByLocationIds(newIds);
    },
    [locationIds]
  );

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="outline" size="sm" className="h-9 gap-2">
          <MapPin className="h-4 w-4" />
          <span>Локации</span>
          {locationIds.length > 0 && (
            <Badge
              variant="secondary"
              className="ml-1 px-1.5 h-5 min-w-5 flex items-center justify-center text-[10px]"
            >
              {locationIds.length}
            </Badge>
          )}
          <ChevronDown className="h-4 w-4 opacity-50" />
        </Button>
      </PopoverTrigger>

      <PopoverContent className="w-80 p-3" align="start">
        <div className="space-y-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground" />
            <Input
              placeholder="Поиск локации..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-8 h-8 text-sm"
            />
          </div>

          {locationIds.length > 0 && (
            <div className="flex flex-wrap gap-1 max-h-20 overflow-y-auto p-1">
              {locationIds.map((id) => {
                const loc = locations.find((l) => l.id === id);
                return loc ? (
                  <Badge
                    key={id}
                    variant="secondary"
                    className="text-[10px] gap-1 pr-1 cursor-pointer"
                    onClick={() => handleChange(id, false)}
                  >
                    {loc.name}
                    <X className="h-3 w-3" />
                  </Badge>
                ) : null;
              })}
            </div>
          )}

          <ScrollArea className="h-62.5 pr-3">
            {isLoading ? (
              <div className="flex justify-center p-4">
                <Spinner className="h-5 w-5" />
              </div>
            ) : (
              <div className="space-y-1">
                {filteredLocations.map((loc) => {
                  const isChecked = locationIds.includes(loc.id);
                  return (
                    <div
                      key={loc.id}
                      onClick={() => handleChange(loc.id, !isChecked)}
                      className={cn(
                        "flex items-center justify-between px-2 py-1.5 rounded-md cursor-pointer text-sm transition-colors",
                        isChecked
                          ? "bg-accent text-accent-foreground"
                          : "hover:bg-muted"
                      )}
                    >
                      <span className="truncate mr-2">{loc.name}</span>
                      {isChecked && <Check className="h-4 w-4 shrink-0" />}
                    </div>
                  );
                })}

                {filteredLocations.length === 0 && (
                  <p className="text-xs text-center text-muted-foreground py-4">
                    Локации не найдены
                  </p>
                )}
              </div>
            )}
          </ScrollArea>
        </div>
      </PopoverContent>
    </Popover>
  );
}
