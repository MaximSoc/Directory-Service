import { useDepartmentsList } from "./use-departments-list";
import { Spinner } from "@/shared/components/ui/spinner";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Check, X } from "lucide-react";
import { cn } from "@/shared/lib/utils";
import {
  setDepartmentsSearch,
  useGetDepartmentsFilter,
} from "./departments-filter-store";
import { useDebounce } from "use-debounce";

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
  const { search, isActive } = useGetDepartmentsFilter();

  const [debouncedSearch] = useDebounce(search, 300);

  const { data, isLoading } = useDepartmentsList({
    isActive,
    search: debouncedSearch,
  });

  const departments = data || [];

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setDepartmentsSearch(e.target.value);
  };

  const handleToggle = (id: string) => {
    const isSelected = value.includes(id);
    const nextValue = isSelected
      ? value.filter((v) => v !== id)
      : [...value, id];
    onChange(nextValue);
  };

  const handleRemoveSelected = (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    onChange(value.filter((v) => v !== id));
  };

  return (
    <div className="space-y-3 border rounded-md p-3 bg-card">
      <Input
        placeholder="Поиск подразделений..."
        value={search}
        onChange={handleSearchChange}
        className="h-9 text-sm"
      />

      {value.length > 0 && (
        <div className="flex flex-wrap gap-1 max-h-24 overflow-y-auto bg-muted/30 p-2 rounded-md border border-dashed">
          {value.map((id) => {
            const dep = departments.find((d) => d.id === id);
            return (
              <Badge
                key={id}
                variant="secondary"
                className="pl-2 pr-1 py-0.5 flex items-center gap-1 cursor-default"
              >
                <span className="text-xs">{dep?.name || "Загрузка..."}</span>
                <button
                  type="button"
                  onClick={(e) => handleRemoveSelected(e, id)}
                  className="hover:bg-destructive/20 rounded-full p-0.5 transition-colors"
                >
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            );
          })}
        </div>
      )}

      <div className="h-48 w-full border rounded-md bg-background overflow-y-auto shadow-inner">
        <div className="flex flex-col p-1">
          {isLoading && departments.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-10 gap-2">
              <Spinner className="h-5 w-5" />
              <span className="text-xs text-muted-foreground">
                Загрузка данных...
              </span>
            </div>
          ) : (
            <>
              {departments.map(({ id, name }) => {
                const isChecked = value.includes(id);
                return (
                  <div
                    key={id}
                    className={cn(
                      "flex items-center space-x-2 p-2.5 rounded-sm hover:bg-accent cursor-pointer transition-all",
                      isChecked && "bg-accent/60"
                    )}
                    onClick={() => handleToggle(id)}
                  >
                    <div
                      className={cn(
                        "flex h-4 w-4 shrink-0 items-center justify-center rounded-sm border transition-colors",
                        isChecked
                          ? "bg-primary text-primary-foreground border-primary"
                          : "border-input bg-background"
                      )}
                    >
                      {isChecked && <Check className="h-3 w-3 font-bold" />}
                    </div>

                    <span className="text-sm select-none flex-1 leading-none tracking-tight">
                      {name}
                    </span>
                  </div>
                );
              })}

              {departments.length === 0 && !isLoading && (
                <div className="text-center py-8">
                  <p className="text-sm text-muted-foreground">
                    Ничего не найдено
                  </p>
                  <p className="text-xs text-muted-foreground/60">
                    Попробуйте изменить запрос
                  </p>
                </div>
              )}
            </>
          )}
        </div>
      </div>

      {error && (
        <p className="text-[0.8rem] font-medium text-destructive animate-in fade-in slide-in-from-top-1">
          {error}
        </p>
      )}
    </div>
  );
}
