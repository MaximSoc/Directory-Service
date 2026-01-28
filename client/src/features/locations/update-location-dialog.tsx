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
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useUpdateLocation } from "./model/use-update-location";
import { Location } from "@/entities/locations/types";
import { useEffect } from "react";
import { toast } from "sonner";

const updateLocationSchema = z.object({
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

export type UpdateFormData = z.infer<typeof updateLocationSchema>;

export function UpdateLocationDialog({
  open,
  onOpenChange,
  location,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  location: Location;
}) {
  const { updateLocation, isPending, error } = useUpdateLocation();

  const form = useForm<UpdateFormData>({
    resolver: zodResolver(updateLocationSchema),
  });

  useEffect(() => {
    if (open) {
      form.reset({
        name: location.name,
        country: location.country,
        region: location.region,
        city: location.city,
        postalCode: location.postalCode,
        street: location.street,
        apartamentNumber: location.apartamentNumber,
        timezone: location.timezone,
      });
    }
  }, [open, location, form]);

  const isDirty = form.formState.isDirty;

  const onSubmit = async (data: UpdateFormData) => {
    if (!isDirty) {
      toast.error("Необходимо внести изменения для сохранения");
      return;
    }
    updateLocation(
      { id: location.id, ...data },
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
            <DialogTitle>Редактирование локации</DialogTitle>
            <DialogDescription>
              Заполните форму для редактирования локации
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
                placeholder="Введите название локации"
                {...form.register("name")}
              />
              {form.formState.errors.name && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.name.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="country">Страна</Label>
              <Input
                id="country"
                placeholder="Введите страну"
                {...form.register("country")}
              />
              {form.formState.errors.country && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.country.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="region">Регион</Label>
              <Input
                id="region"
                placeholder="Введите регион"
                {...form.register("region")}
              />
              {form.formState.errors.region && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.region.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="city">Город</Label>
              <Input
                id="city"
                placeholder="Введите город"
                {...form.register("city")}
              />
              {form.formState.errors.city && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.city.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="postalCode">Индекс</Label>
              <Input
                id="postalCode"
                placeholder="Введите индекс"
                {...form.register("postalCode")}
              />
              {form.formState.errors.postalCode && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.postalCode.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="street">Улица</Label>
              <Input
                id="street"
                placeholder="Введите улицу"
                {...form.register("street")}
              />
              {form.formState.errors.street && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.street.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="apartamentNumber">Дом</Label>
              <Input
                id="apartamentNumber"
                placeholder="Введите номер дома"
                {...form.register("apartamentNumber")}
              />
              {form.formState.errors.apartamentNumber && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.apartamentNumber.message}
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="timezone">Временная зона</Label>
              <Input
                id="timezone"
                placeholder="Введите временную зону"
                {...form.register("timezone")}
              />
              {form.formState.errors.timezone && (
                <p className="text-sm text-destructive">
                  {form.formState.errors.timezone.message}
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
