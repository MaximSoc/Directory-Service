import { Department } from "@/entities/departments/types";
import { z } from "zod";
import { useUpdateDepartment } from "./model/use-update-department";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
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
import { DepartmentParentSelect } from "./model/department-parent-select";
import { Loader2, X } from "lucide-react";
import { useWatch } from "react-hook-form";
import { useMoveDepartment } from "./model/use-move-department";

const updateDepartmentSchema = z.object({
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
  parentId: z.string().optional(),
});

export type UpdateDepartmentFormData = z.infer<typeof updateDepartmentSchema>;

export function UpdateDepartmentDialog({
  open,
  onOpenChange,
  department,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  department: Department;
}) {
  const { mutateAsync: moveDepartment, isPending: isMoving } =
    useMoveDepartment();
  const { updateDepartment, isPending: isUpdating } = useUpdateDepartment();

  const form = useForm<UpdateDepartmentFormData>({
    resolver: zodResolver(updateDepartmentSchema),
    defaultValues: {
      name: department.name,
      identifier: department.identifier || "",
      parentId: department.parentId || "",
    },
  });

  const { isDirty, errors } = form.formState;

  const currentParentId = useWatch({
    control: form.control,
    name: "parentId",
  });

  const onSubmit = async (data: UpdateDepartmentFormData) => {
    const newParentId = data.parentId === "" ? null : data.parentId;
    const oldParentId = department.parentId || null;

    try {
      if (newParentId !== oldParentId) {
        if (newParentId === department.id) {
          toast.error(
            "Подразделение не может быть своим собственным родителем"
          );
          return;
        }

        await moveDepartment({
          departmentId: department.id,
          parentId: newParentId,
        });
      }

      if (
        data.name !== department.name ||
        data.identifier !== department.identifier
      ) {
        updateDepartment({
          departmentId: department.id,
          name: data.name,
          identifier: data.identifier,
        });
      }
      toast.success("Изменения сохранены");
      onOpenChange(false);
      form.reset(data);
    } catch (e) {}
  };

  const isLoading = isUpdating || isMoving;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-125">
        <DialogHeader>
          <DialogTitle>Редактирование подразделения</DialogTitle>
          <DialogDescription>
            Измените данные подразделения или его положение в структуре
          </DialogDescription>
        </DialogHeader>

        <form className="space-y-4 py-4" onSubmit={form.handleSubmit(onSubmit)}>
          <div className="space-y-2">
            <Label htmlFor="name">Название</Label>
            <Input
              id="name"
              disabled={isLoading}
              placeholder="Введите название подразделения"
              {...form.register("name")}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="identifier">Идентификатор</Label>
            <Input
              id="identifier"
              disabled={isLoading}
              placeholder="Введите идентификатор"
              {...form.register("identifier")}
            />
            {errors.identifier && (
              <p className="text-sm text-destructive">
                {errors.identifier.message}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label htmlFor="parentId">Родительское подразделение</Label>
              {currentParentId && (
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="h-6 px-2 text-[10px] text-muted-foreground hover:text-destructive"
                  onClick={() =>
                    form.setValue("parentId", "", { shouldDirty: true })
                  }
                >
                  <X className="mr-1 h-3 w-3" /> Сбросить (сделать корневым)
                </Button>
              )}
            </div>
            <Controller
              control={form.control}
              name="parentId"
              render={({ field }) => (
                <DepartmentParentSelect
                  value={field.value || ""}
                  onChange={field.onChange}
                />
              )}
            />
            <p className="text-[10px] text-muted-foreground italic">
              При смене родителя вся ветка подразделения будет перемещена
            </p>
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              Отмена
            </Button>
            <Button type="submit" disabled={isLoading || !isDirty}>
              {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Сохранить изменения
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
