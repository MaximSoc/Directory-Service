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
import { useCreateLocation } from "./model/use-create-location";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

const createLocationSchema = z.object({
  name: z
    .string()
    .min(1, "Название локации обязательно")
    .min(3, "Название должно содержать минимум 3 символа")
    .max(120, "Название не должно превышать 120 символов"),
  country: z.string().min(1, "Укажите страну"),
  region: z.string().min(1, "Укажите регион"),
  city: z.string().min(1, "Укажите город"),
  postalCode: z.string().min(1, "Укажите почтовый индекс"),
  street: z.string().min(1, "Укажите улицу"),
  apartamentNumber: z.string().min(1, "Укажите номер дома"),
  timezone: z.string().min(1, "Укажите временную зону"),
});

type CreateFormData = z.infer<typeof createLocationSchema>;

export function CreateLocationDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const initialData: CreateFormData = {
    name: "",
    country: "",
    region: "",
    city: "",
    postalCode: "",
    street: "",
    apartamentNumber: "",
    timezone: "",
  };
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateFormData>({
    defaultValues: initialData,
    resolver: zodResolver(createLocationSchema),
  });

  const { createLocation, isPending, error } = useCreateLocation();

  const onSubmit = async (data: CreateFormData) => {
    createLocation(data, {
      onSuccess: () => {
        onOpenChange(false);
        reset(initialData);
      },
    });
  };

  return (
    <div>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-125">
          <DialogHeader>
            <DialogTitle>Создание локации</DialogTitle>
            <DialogDescription>
              Заполните форму для создания локации
            </DialogDescription>
          </DialogHeader>
          <form className="space-y-4 py-4" onSubmit={handleSubmit(onSubmit)}>
            <div className="space-y-2">
              <Label htmlFor="name">Название</Label>
              <Input
                id="name"
                placeholder="Введите название локации"
                {...register("name")}
              />
              {errors.name && (
                <p className="text-sm text-destructive">
                  {errors.name.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="country">Страна</Label>
              <Input
                id="country"
                placeholder="Введите страну"
                {...register("country")}
              />
              {errors.country && (
                <p className="text-sm text-destructive">
                  {errors.country.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="region">Регион</Label>
              <Input
                id="region"
                placeholder="Введите регион"
                {...register("region")}
              />
              {errors.region && (
                <p className="text-sm text-destructive">
                  {errors.region.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="city">Город</Label>
              <Input
                id="city"
                placeholder="Введите город"
                {...register("city")}
              />
              {errors.city && (
                <p className="text-sm text-destructive">
                  {errors.city.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="postalCode">Индекс</Label>
              <Input
                id="postalCode"
                placeholder="Введите индекс"
                {...register("postalCode")}
              />
              {errors.postalCode && (
                <p className="text-sm text-destructive">
                  {errors.postalCode.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="street">Улица</Label>
              <Input
                id="street"
                placeholder="Введите улицу"
                {...register("street")}
              />
              {errors.street && (
                <p className="text-sm text-destructive">
                  {errors.street.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="apartamentNumber">Дом</Label>
              <Input
                id="apartamentNumber"
                placeholder="Введите номер дома"
                {...register("apartamentNumber")}
              />
              {errors.apartamentNumber && (
                <p className="text-sm text-destructive">
                  {errors.apartamentNumber.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="timezone">Временная зона</Label>
              <Input
                id="timezone"
                placeholder="Введите временную зону"
                {...register("timezone")}
              />
              {errors.timezone && (
                <p className="text-sm text-destructive">
                  {errors.timezone.message}
                </p>
              )}
            </div>

            <div className="flex justify-end space-x-2">
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
              {error && <div>{error.message}</div>}
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
