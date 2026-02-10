import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { z } from "zod";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { useUpdatePosition } from "./model/use-update-position";
import { Position } from "@/entities/positions/types";
import { DepartmentsMultiSelect } from "../departments/model/departments-multi-select";

const updatePositionSchema = z.object({
  name: z
    .string()
    .min(1, "Название должности обязательно")
    .min(3, "Название должно содержать минимум 3 символа")
    .max(100, "Название не должно превышать 100 символов"),
  description: z
    .string()
    .max(1000, "Описание не должно превышать 1000 символов")
    .optional(),
  departmentsIds: z
    .array(z.string())
    .min(1, "Выберите хотя бы одно подразделение"),
});

export type UpdatePositionFormData = z.infer<typeof updatePositionSchema>;

export function UpdatePositionDialog({
  open,
  onOpenChange,
  position,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  position: Position;
}) {
  const { updatePosition, isPending, error } = useUpdatePosition();

  const form = useForm<UpdatePositionFormData>({
    resolver: zodResolver(updatePositionSchema),
    defaultValues: {
      name: position.name,
      description: position.description || "",
      departmentsIds: position.departmentIds || [],
    },
  });

  const isDirty = form.formState.isDirty;

  const onSubmit = async (data: UpdatePositionFormData) => {
    if (!isDirty) {
      toast.error("Необходимо внести изменения для сохранения");
      return;
    }
    updatePosition(
      { id: position.id, ...data },
      {
        onSuccess: () => {
          onOpenChange(false);
        },
      }
    );
  };

  return (
    <div>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-125">
          <DialogHeader>
            <DialogTitle>Редактирование должности</DialogTitle>
            <DialogDescription>
              Заполните форму для редактирования должности
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
                placeholder="Введите название должности"
                {...form.register("name")}
              />
              {form.formState.errors.name && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.name.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">Описание</Label>
              <Input
                id="description"
                placeholder="Введите описание"
                {...form.register("description")}
              />
              {form.formState.errors.description && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.description.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label>Подразделения</Label>
              <Controller
                control={form.control}
                name="departmentsIds"
                render={({ field }) => (
                  <DepartmentsMultiSelect
                    value={field.value}
                    onChange={field.onChange}
                    error={form.formState.errors.departmentsIds?.message}
                  />
                )}
              />
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
              {error && <div>{error.message}</div>}
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
