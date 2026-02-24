import { useCallback, useMemo, useState } from "react";
import { useQueryDepartmentsList } from "../departments/model/use-query-departments-list";
import { Spinner } from "@/shared/components/ui/spinner";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Circle, X } from "lucide-react";
import { ScrollArea } from "@/shared/components/ui/scroll-area";
import {
  setDepartmentsFilterParentId,
  useGetDepartmentsFilter,
} from "./model/departments-filter-store";
import { cn } from "@/shared/lib/utils";

const initialData = {
  isActive: undefined,
  search: "",
  page: 1,
  pageSize: 1000,
};

export function ParentDepartmentFilter() {
  const { data: departments = [], isLoading } =
    useQueryDepartmentsList(initialData);
  const { parentId } = useGetDepartmentsFilter();
  const [searchTerm, setSearchTerm] = useState("");

  const filteredDepartments = useMemo(
    () =>
      departments.filter((dep) =>
        dep.name.toLowerCase().includes(searchTerm.toLowerCase())
      ),
    [departments, searchTerm]
  );

  const selectedDepartment = useMemo(
    () => departments.find((d) => d.id === parentId),
    [departments, parentId]
  );

  const handleSelect = useCallback(
    (id: string) => {
      const nextId = parentId === id ? undefined : id;
      setDepartmentsFilterParentId(nextId);
    },
    [parentId]
  );

  const handleClear = () => setDepartmentsFilterParentId(undefined);

  if (isLoading) return <Spinner />;

  return (
    <div className="space-y-4">
      <Input
        placeholder="Поиск родительского подразделения..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="max-w-full"
      />

      <div className="flex flex-wrap gap-1 min-h-10 items-center p-2 bg-muted/50 rounded-md border border-dashed">
        {selectedDepartment ? (
          <Badge
            variant="default"
            className="pl-3 pr-1 py-1 gap-1 cursor-pointer"
            onClick={handleClear}
          >
            {selectedDepartment.name}
            <X className="h-3 w-3 hover:text-destructive transition-colors" />
          </Badge>
        ) : (
          <span className="text-xs text-muted-foreground ml-2">
            Родительское подразделение не выбрано
          </span>
        )}
      </div>

      <ScrollArea className="h-64 w-full rounded-md border">
        <div className="flex flex-col p-1">
          {filteredDepartments.map(({ id, name, identifier }) => {
            const isSelected = parentId === id;

            return (
              <button
                key={id}
                onClick={() => handleSelect(id)}
                className={cn(
                  "flex items-center justify-between w-full px-3 py-2 text-sm rounded-sm transition-colors text-left",
                  isSelected
                    ? "bg-primary/10 text-primary font-medium"
                    : "hover:bg-accent hover:text-accent-foreground"
                )}
              >
                <div className="flex flex-col">
                  <span>{name}</span>
                  <span className="text-[10px] opacity-70 font-mono">
                    {identifier}
                  </span>
                </div>
                {isSelected && <Circle className="h-3 w-3 fill-current" />}
              </button>
            );
          })}

          {filteredDepartments.length === 0 && (
            <p className="text-sm text-muted-foreground text-center py-8">
              Ничего не найдено
            </p>
          )}
        </div>
      </ScrollArea>
    </div>
  );
}
