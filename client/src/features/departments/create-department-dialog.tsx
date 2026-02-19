import { z } from "zod";
import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Label } from "@/shared/components/ui/label";
import { Input } from "@/shared/components/ui/input";
import { Button } from "@/shared/components/ui/button";
import { useCreateDepartment } from "./model/use-create-department";
import { LocationsMultiSelect } from "../locations/model/locations-multi-select";
import { DepartmentParentSelect } from "./model/department-parent-select";
import { useQueryDepartmentsList } from "./model/use-query-departments-list";
import { Fragment, useMemo } from "react";
import { ChevronRight, Layers, MapPinned } from "lucide-react";
import { cn } from "@/shared/lib/utils";

const createDepartmentSchema = z.object({
  name: z
    .string()
    .min(1, "Название подразделения обязательно")
    .min(3, "Название должно содержать минимум 3 символа")
    .max(150, "Название не должно превышать 150 символов"),
  identifier: z
    .string()
    .min(1, "Идентификатор подразделения обязателен")
    .min(3, "Идентификатор должен содержать минимум 3 символа")
    .max(1000, "Идентификатор не должен превышать 1000 символов"),
  locationIds: z.array(z.string()).min(1, "Выберите хотя бы одну локацию"),
  parentId: z.string().optional().nullable(),
});

type CreateFormData = z.infer<typeof createDepartmentSchema>;

export function CreateDepartmentDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const initialData: CreateFormData = {
    name: "",
    identifier: "",
    locationIds: [],
    parentId: "",
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    control,
  } = useForm<CreateFormData>({
    defaultValues: initialData,
    resolver: zodResolver(createDepartmentSchema),
  });

  const { createDepartment, isPending, error } = useCreateDepartment();

  const onSubmit = (data: CreateFormData) => {
    createDepartment(
      {
        name: data.name,
        identifier: data.identifier,
        locationIds: data.locationIds,
        parentId: data.parentId || undefined,
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          reset(initialData);
        },
      }
    );
  };

  const { data: departments } = useQueryDepartmentsList({
    search: "",
    isActive: true,
  });

  const selectedParentId = useWatch({ control, name: "parentId" });
  const identifierValue = useWatch({ control, name: "identifier" });

  const previewData = useMemo(() => {
    const currentPathPart = identifierValue || "новое";

    if (!selectedParentId) {
      return {
        path: currentPathPart,
        depth: 0,
      };
    }

    const parent = departments?.find((d) => d.id === selectedParentId);

    if (!parent) {
      return {
        path: `... . ${currentPathPart}`,
        depth: 0,
      };
    }

    return {
      path: `${parent.path}.${currentPathPart}`,
      depth: parent.depth + 1,
    };
  }, [selectedParentId, departments, identifierValue]);

  return (
    <div>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-125">
          <DialogHeader>
            <DialogTitle>Создание подразделения</DialogTitle>
            <DialogDescription>
              Заполните форму для создания нового подразделения
            </DialogDescription>
          </DialogHeader>

          <form className="space-y-4 py-4" onSubmit={handleSubmit(onSubmit)}>
            <div className="space-y-2">
              <Label htmlFor="name">
                Название <span className="text-destructive">*</span>
              </Label>
              <Input
                id="name"
                placeholder="Введите название подразделения"
                {...register("name")}
              />
              {errors.name && (
                <p className="text-sm text-destructive">
                  {errors.name.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="identifier">
                Идентификатор <span className="text-destructive">*</span>
              </Label>
              <Input
                id="identifier"
                placeholder="Введите идентификатор"
                {...register("identifier")}
              />
              {errors.identifier && (
                <p className="text-sm text-destructive">
                  {errors.identifier.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label>
                Локации <span className="text-destructive">*</span>
              </Label>
              <Controller
                control={control}
                name="locationIds"
                render={({ field }) => (
                  <LocationsMultiSelect
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.locationIds?.message}
                  />
                )}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="parentId">Родительское подразделение</Label>
              <Controller
                control={control}
                name="parentId"
                render={({ field }) => (
                  <DepartmentParentSelect
                    value={field.value || ""}
                    onChange={field.onChange}
                  />
                )}
              />
              <p className="text-[10px] text-muted-foreground">
                Оставьте пустым, если это корневое подразделение
              </p>
            </div>

            <div className="rounded-lg border bg-card text-card-foreground shadow-sm overflow-hidden">
              <div className="bg-muted/50 px-3 py-1.5 border-b flex justify-between items-center">
                <span className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground flex items-center gap-1.5">
                  <MapPinned className="h-3 w-3" /> Структура иерархии
                </span>
                <div className="flex items-center gap-1 bg-background px-2 py-0.5 rounded-full border text-[10px] font-medium">
                  <Layers className="h-3 w-3 text-primary" />
                  <span>Уровень {previewData.depth}</span>
                </div>
              </div>

              <div className="p-3">
                <div className="flex flex-wrap items-center gap-1 text-sm font-medium leading-none">
                  {previewData.path.split(".").map((segment, index, array) => (
                    <Fragment key={index}>
                      <span
                        className={cn(
                          "px-1.5 py-0.5 rounded-md transition-colors",
                          index === array.length - 1
                            ? "bg-primary/10 text-primary ring-1 ring-inset ring-primary/20"
                            : "text-muted-foreground"
                        )}
                      >
                        {segment}
                      </span>
                      {index < array.length - 1 && (
                        <ChevronRight className="h-3.5 w-3.5 text-muted-foreground/50" />
                      )}
                    </Fragment>
                  ))}
                </div>
                <p className="mt-2 text-[10px] text-muted-foreground italic">
                  * Идентификатор используется для формирования системного пути
                  в БД
                </p>
              </div>
            </div>

            <div className="flex justify-end space-x-2 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Отмена
              </Button>
              <Button type="submit" disabled={isPending}>
                Создать
              </Button>
            </div>
            {error && (
              <div className="text-sm text-destructive text-right">
                Произошла ошибка при создании
              </div>
            )}
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
