import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Location } from "@/entities/locations/types";
import { toast } from "sonner";
import { useDeleteLocation } from "./model/use-delete-location";

export function DeleteLocationDialog({
  open,
  onOpenChange,
  location,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  location: Location;
}) {
  const { deleteLocation, isPending } = useDeleteLocation();

  const handleDelete = () => {
    deleteLocation(
      { locationId: location.id },
      {
        onSuccess: () => {
          toast.success("Локация удалена");
          onOpenChange(false);
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Удалить локацию</DialogTitle>
          <DialogDescription>
            Вы уверены, что хотите удалить локацию &quot;
            <strong>{location.name}</strong>
            &quot;? Это действие нельзя отменить.
          </DialogDescription>
        </DialogHeader>
        <div className="flex justify-end space-x-2 pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isPending}
          >
            Отмена
          </Button>
          <Button
            variant="destructive"
            onClick={handleDelete}
            disabled={isPending}
          >
            {isPending ? "Удаление..." : "Удалить"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
