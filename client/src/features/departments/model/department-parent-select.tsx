"use client";

import { Check, Search, X } from "lucide-react";
import { cn } from "@/shared/lib/utils";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/shared/components/ui/command";
import { ScrollArea } from "@/shared/components/ui/scroll-area";
import { useQueryDepartmentsList } from "./use-query-departments-list";
import { Spinner } from "@/shared/components/ui/spinner";
import { useMemo, useState } from "react";

export function DepartmentParentSelect({
  value,
  onChange,
}: {
  value: string;
  onChange: (val: string) => void;
}) {
  const [isPickerOpen, setIsPickerOpen] = useState(false);

  const { data: departments = [], isLoading } = useQueryDepartmentsList({
    search: "",
    isActive: true,
  });

  const selectedDepartment = useMemo(
    () => departments.find((dep) => dep.id === value),
    [departments, value]
  );

  return (
    <>
      <div className="flex gap-2">
        <Button
          type="button"
          variant="outline"
          className="w-full justify-between font-normal bg-background"
          onClick={() => setIsPickerOpen(true)}
        >
          {value === "" ? (
            <span className="text-muted-foreground text-sm">
              Нет (корневое)
            </span>
          ) : selectedDepartment ? (
            <span className="truncate">
              {selectedDepartment.name}
              <span className="ml-2 text-[10px] opacity-50 font-mono">
                ({selectedDepartment.path})
              </span>
            </span>
          ) : (
            "Выберите подразделение..."
          )}
          <Search className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>

        {value && (
          <Button
            type="button"
            variant="ghost"
            size="icon"
            onClick={() => onChange("")}
            className="shrink-0"
          >
            <X className="h-4 w-4" />
          </Button>
        )}
      </div>

      <Dialog open={isPickerOpen} onOpenChange={setIsPickerOpen}>
        <DialogContent className="sm:max-w-125 p-0 gap-0 overflow-hidden max-h-[90vh] h-125 flex flex-col">
          <DialogHeader className="p-4 border-b shrink-0 bg-background z-10">
            <DialogTitle>Выбор родительского подразделения</DialogTitle>
          </DialogHeader>

          <Command className="flex flex-col flex-1 min-h-0 rounded-none bg-background">
            <div className="border-b px-3 shrink-0 bg-background z-10">
              <CommandInput
                placeholder="Введите название или путь..."
                autoFocus
                className="h-12 border-none focus:ring-0"
              />
            </div>

            <CommandList className="flex-1 overflow-hidden p-0">
              <ScrollArea className="h-full">
                <div className="p-2">
                  {" "}
                  {isLoading ? (
                    <div className="flex items-center justify-center py-10">
                      <Spinner className="h-6 w-6" />
                    </div>
                  ) : (
                    <>
                      <CommandEmpty className="py-6 text-center text-sm">
                        Подразделения не найдены.
                      </CommandEmpty>

                      <CommandGroup heading="Системные">
                        <CommandItem
                          value="root-none"
                          onSelect={() => {
                            onChange("");
                            setIsPickerOpen(false);
                          }}
                          className="cursor-pointer"
                        >
                          <Check
                            className={cn(
                              "mr-2 h-4 w-4 text-primary",
                              value === "" ? "opacity-100" : "opacity-0"
                            )}
                          />
                          <div>
                            <p className="font-medium">Без родителя</p>
                            <p className="text-[10px] text-muted-foreground">
                              Корневое подразделение
                            </p>
                          </div>
                        </CommandItem>
                      </CommandGroup>

                      <CommandGroup heading="Все подразделения">
                        {departments.map((dep) => (
                          <CommandItem
                            key={dep.id}
                            value={`${dep.name} ${dep.path}`}
                            onSelect={() => {
                              onChange(dep.id);
                              setIsPickerOpen(false);
                            }}
                            className="cursor-pointer"
                          >
                            <Check
                              className={cn(
                                "mr-2 h-4 w-4 text-primary",
                                value === dep.id ? "opacity-100" : "opacity-0"
                              )}
                            />
                            <div className="flex flex-col">
                              <span className="font-medium text-sm text-foreground">
                                {dep.name}
                              </span>
                              <span className="text-[10px] text-muted-foreground font-mono truncate">
                                {dep.path}
                              </span>
                            </div>
                          </CommandItem>
                        ))}
                      </CommandGroup>
                    </>
                  )}
                </div>
              </ScrollArea>
            </CommandList>
          </Command>

          <div className="p-2 border-t bg-muted/30 flex justify-end shrink-0">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setIsPickerOpen(false)}
            >
              Закрыть
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
