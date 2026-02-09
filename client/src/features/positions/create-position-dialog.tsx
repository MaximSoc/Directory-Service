import { z } from "zod";
import { useCreatePosition } from "./model/use-create-position";
import { Controller, useForm } from "react-hook-form";
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
import { DepartmentsMultiSelect } from "../departments/model/departments-multi-select";
import { Button } from "@/shared/components/ui/button";

const createPositionSchema = z.object({
  name: z
    .string()
    .min(1, "Название локации обязательно")
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

type CreateFormData = z.infer<typeof createPositionSchema>;

export function CreatePositionDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const initialData: CreateFormData = {
    name: "",
    description: "",
    departmentsIds: [],
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    control,
  } = useForm<CreateFormData>({
    defaultValues: initialData,
    resolver: zodResolver(createPositionSchema),
  });

  const { createPosition, isPending, error } = useCreatePosition();

  const onSubmit = (data: CreateFormData) => {
    createPosition(
      {
        name: data.name,
        description: data.description || undefined,
        departmentsIds: data.departmentsIds,
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          reset(initialData);
        },
      }
    );
  };

  return (
    <div>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-125">
          <DialogHeader>
            <DialogTitle>Создание должности</DialogTitle>
            <DialogDescription>
              Заполните форму для создания новой должности
            </DialogDescription>
          </DialogHeader>

          <form className="space-y-4 py-4" onSubmit={handleSubmit(onSubmit)}>
            <div className="space-y-2">
              <Label htmlFor="name">
                Название <span className="text-destructive">*</span>
              </Label>
              <Input
                id="name"
                placeholder="Введите название должности"
                {...register("name")}
              />
              {errors.name && (
                <p className="text-sm text-destructive">
                  {errors.name.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Описание</Label>
              <Input
                id="description"
                placeholder="Введите описание (необязательно)"
                {...register("description")}
              />
              {errors.description && (
                <p className="text-sm text-destructive">
                  {errors.description.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label>
                Подразделения <span className="text-destructive">*</span>
              </Label>
              <Controller
                control={control}
                name="departmentsIds"
                render={({ field }) => (
                  <DepartmentsMultiSelect
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.departmentsIds?.message}
                  />
                )}
              />
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
