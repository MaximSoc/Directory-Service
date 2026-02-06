import { useCallback, useMemo, useState } from "react";
import { useDepartmentsList } from "../departments/model/use-departments-list";
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
  const { data: departments = [], isLoading } = useDepartmentsList();
  const { departmentIds } = useGetPositionFilter();
  const [searchTerm, setSearchTerm] = useState("");

  const filteredDepartments = useMemo(
    () =>
      departments.filter((dep) =>
        dep.name.toLowerCase().includes(searchTerm.toLowerCase())
      ),
    [departments, searchTerm]
  );

  const handleChange = useCallback(
    (id: string, checked: boolean) => {
      const newIds = checked
        ? [...departmentIds, id]
        : departmentIds.filter((d) => d !== id);
      setPositionFilterByDepartments(newIds);
    },
    [departmentIds]
  );

  if (isLoading) return <Spinner />;

  return (
    <div className="space-y-4">
      <Input
        placeholder="Поиск подразделений..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="max-w-sm"
      />

      <div className="flex flex-wrap gap-1 max-h-20 overflow-auto p-2 bg-muted/50 rounded-md">
        {departmentIds.map((id) => {
          const dep = departments.find((d) => d.id === id);
          return dep ? (
            <Badge
              key={id}
              variant="secondary"
              className="cursor-pointer hover:opacity-80"
              onClick={() => handleChange(id, false)}
            >
              {dep.name}
              <X className="h-3 w-3 ml-1" />
            </Badge>
          ) : null;
        })}
      </div>

      <ScrollArea className="h-64 w-full">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-2 p-2">
          {filteredDepartments.map(({ id, name }) => (
            <div
              key={id}
              className="flex items-center space-x-2 p-2 rounded-md hover:bg-accent cursor-pointer"
            >
              <Checkbox
                id={`dep-${id}`}
                checked={departmentIds.includes(id)}
                onCheckedChange={(checked) => handleChange(id, !!checked)}
              />
              <label
                htmlFor={`dep-${id}`}
                className="text-sm cursor-pointer select-none truncate"
                title={name}
              >
                {name}
              </label>
            </div>
          ))}
        </div>
      </ScrollArea>

      {filteredDepartments.length === 0 && searchTerm && (
        <p className="text-sm text-muted-foreground text-center py-8">
          Подразделения не найдены
        </p>
      )}
    </div>
  );
}
