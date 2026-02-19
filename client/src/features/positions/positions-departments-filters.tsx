import { useCallback, useMemo, useState } from "react";
import { useQueryDepartmentsList } from "../departments/model/use-query-departments-list";
import { Spinner } from "@/shared/components/ui/spinner";
import { Checkbox } from "@/shared/components/ui/checkbox";
import {
  setPositionFilterByDepartments,
  useGetPositionFilter,
} from "./model/positions-filter-store";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { X } from "lucide-react";
import { ScrollArea } from "@/shared/components/ui/scroll-area";

export function PositionDepartmentsFilter() {
  const [searchTerm, setSearchTerm] = useState("");

  const { data: allDepartments = [], isLoading } = useQueryDepartmentsList({
    search: "",
    isActive: undefined,
  });
  const { departmentIds } = useGetPositionFilter();

  const filteredDepartments = useMemo(() => {
    if (!searchTerm.trim()) return allDepartments;

    const lowerSearch = searchTerm.toLowerCase();
    return allDepartments.filter(
      (dep) =>
        dep.name.toLowerCase().includes(lowerSearch) ||
        dep.identifier.toLowerCase().includes(lowerSearch)
    );
  }, [allDepartments, searchTerm]);

  const handleChange = useCallback(
    (id: string, checked: boolean) => {
      const newIds = checked
        ? [...departmentIds, id]
        : departmentIds.filter((d) => d !== id);
      setPositionFilterByDepartments(newIds);
    },
    [departmentIds]
  );

  return (
    <div className="space-y-4">
      <div className="relative">
        <Input
          placeholder="Поиск подразделений..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-9 max-w-sm"
          autoFocus
        />
      </div>

      <div className="flex flex-wrap gap-1 min-h-10 p-2 bg-muted/30 rounded-md border border-dashed">
        {departmentIds.length > 0 ? (
          departmentIds.map((id) => {
            const dep = allDepartments.find((d) => d.id === id);
            return dep ? (
              <Badge
                key={id}
                variant="secondary"
                className="pl-2 pr-1 py-1 flex items-center gap-1 cursor-pointer hover:bg-destructive/10 transition-colors"
                onClick={() => handleChange(id, false)}
              >
                {dep.name}
                <X className="h-3 w-3" />
              </Badge>
            ) : null;
          })
        ) : (
          <span className="text-xs text-muted-foreground p-1">
            Ничего не выбрано
          </span>
        )}
      </div>

      <ScrollArea className="h-72 w-full rounded-md border">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center h-full py-10 gap-2">
            <Spinner className="h-6 w-6" />
            <span className="text-sm text-muted-foreground">
              Загрузка структуры...
            </span>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-1 p-2">
            {filteredDepartments.map(({ id, name, identifier }) => {
              const isChecked = departmentIds.includes(id);
              return (
                <div
                  key={id}
                  onClick={() => handleChange(id, !isChecked)}
                  className="flex items-center space-x-2 p-2.5 rounded-md hover:bg-accent cursor-pointer transition-colors"
                >
                  <Checkbox
                    id={`dep-${id}`}
                    checked={isChecked}
                    onCheckedChange={(checked) => handleChange(id, !!checked)}
                    onClick={(e) => e.stopPropagation()}
                  />
                  <div className="flex flex-col min-w-0">
                    <span className="text-sm font-medium truncate leading-none mb-1">
                      {name}
                    </span>
                    <span className="text-[10px] text-muted-foreground font-mono">
                      {identifier}
                    </span>
                  </div>
                </div>
              );
            })}

            {filteredDepartments.length === 0 && (
              <div className="col-span-full py-10 text-center">
                <p className="text-sm text-muted-foreground">
                  Подразделения не найдены
                </p>
              </div>
            )}
          </div>
        )}
      </ScrollArea>
    </div>
  );
}
