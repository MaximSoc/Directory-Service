import { useMemo, useState } from "react";
import { useDepartmentsList } from "./use-departments-list";
import { Spinner } from "@/shared/components/ui/spinner";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Check, X } from "lucide-react";
import { cn } from "@/shared/lib/utils";

interface DepartmentsMultiSelectProps {
  value: string[];
  onChange: (value: string[]) => void;
  error?: string;
}

export function DepartmentsMultiSelect({
  value = [],
  onChange,
  error,
}: DepartmentsMultiSelectProps) {
  const { data: departments = [], isLoading } = useDepartmentsList();
  const [searchTerm, setSearchTerm] = useState("");

  const filteredDepartments = useMemo(
    () =>
      departments.filter((dep) =>
        dep.name.toLowerCase().includes(searchTerm.toLowerCase())
      ),
    [departments, searchTerm]
  );

  const handleToggle = (id: string) => {
    const isSelected = value.includes(id);
    const newIds = isSelected ? value.filter((d) => d !== id) : [...value, id];
    onChange(newIds);
  };

  const handleRemove = (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    const newIds = value.filter((d) => d !== id);
    onChange(newIds);
  };

  if (isLoading) return <Spinner />;

  return (
    <div className="space-y-3 border rounded-md p-3">
      <Input
        placeholder="Поиск подразделений..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="h-8 text-sm"
      />

      {value.length > 0 && (
        <div className="flex flex-wrap gap-1 max-h-20 overflow-auto bg-muted/30 p-1 rounded">
          {value.map((id) => {
            const dep = departments.find((d) => d.id === id);
            return dep ? (
              <Badge
                key={id}
                variant="secondary"
                className="cursor-pointer hover:bg-destructive hover:text-destructive-foreground transition-colors"
                onClick={(e) => handleRemove(e, id)}
              >
                {dep.name}
                <X className="h-3 w-3 ml-1" />
              </Badge>
            ) : null;
          })}
        </div>
      )}

      <div className="h-40 w-full border rounded-md bg-background overflow-y-auto">
        <div className="grid grid-cols-1 gap-1 p-2">
          {filteredDepartments.map(({ id, name }) => {
            const isChecked = value.includes(id);
            return (
              <div
                key={id}
                className={cn(
                  "flex items-center space-x-2 p-2 rounded-md hover:bg-accent cursor-pointer transition-colors",
                  isChecked && "bg-accent"
                )}
                onClick={() => handleToggle(id)}
              >
                <div
                  className={cn(
                    "flex h-4 w-4 shrink-0 items-center justify-center rounded-sm border transition-colors",
                    isChecked
                      ? "bg-primary text-primary-foreground border-primary"
                      : "border-input bg-background text-transparent"
                  )}
                >
                  {isChecked && <Check className="h-3 w-3 font-bold" />}
                </div>

                <span className="text-sm select-none flex-1 leading-none">
                  {name}
                </span>
              </div>
            );
          })}
          {filteredDepartments.length === 0 && (
            <p className="text-xs text-muted-foreground text-center py-4">
              Ничего не найдено
            </p>
          )}
        </div>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}
    </div>
  );
}
