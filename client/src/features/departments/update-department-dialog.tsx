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
import { X } from "lucide-react";
import { useWatch } from "react-hook-form";

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
  const { updateDepartment, isPending, error } = useUpdateDepartment();

  const form = useForm<UpdateDepartmentFormData>({
    resolver: zodResolver(updateDepartmentSchema),
    defaultValues: {
      name: department.name,
      identifier: department.identifier || "",
      parentId: department.parentId || "",
    },
  });

  const isDirty = form.formState.isDirty;

  const currentParentId = useWatch({
    control: form.control,
    name: "parentId",
  });

  const onSubmit = async (data: UpdateDepartmentFormData) => {
    const parentIdValue = data.parentId === "" ? null : data.parentId;

    if (parentIdValue === department.id) {
      toast.error("Подразделение не может быть своим собственным родителем");
      return;
    }

    updateDepartment(
      {
        departmentId: department.id,
        ...data,
        parentId: parentIdValue as string,
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          form.reset(data);
        },
      }
    );
  };

  return (
    <div>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-125">
          <DialogHeader>
            <DialogTitle>Редактирование подразделения</DialogTitle>
            <DialogDescription>
              Заполните форму для редактирования подразделения
            </DialogDescription>
          </DialogHeader>
          <form
            className="space-y-4 py-4"
            onSubmit={form.handleSubmit(onSubmit)}
          >
            <div className="space-y-2">
              <Label htmlFor="name">Название</Label>
              <Input
                id="name"
                placeholder="Введите название подразделения"
                {...form.register("name")}
              />
              {form.formState.errors.name && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.name.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">Идентификатор</Label>
              <Input
                id="identifier"
                placeholder="Введите идентификатор"
                {...form.register("identifier")}
              />
              {form.formState.errors.identifier && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.identifier.message}
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
                    <X className="mr-1 h-3 w-3" /> Сбросить (корневое)
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
                Оставьте пустым, если подразделение должно быть корневым
              </p>
            </div>

            <div className="flex justify-end space-x-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Отмена
              </Button>
              <Button type="submit" disabled={isPending || !isDirty}>
                Изменить
              </Button>
              {error && (
                <p className="text-sm text-destructive mt-2 text-right">
                  {error.message}
                </p>
              )}
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
