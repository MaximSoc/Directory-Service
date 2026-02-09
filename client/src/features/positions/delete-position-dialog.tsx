import { Position } from "@/entities/positions/types";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { toast } from "sonner";
import { useDeletePosition } from "./model/use-delete-position";

export function DeletePositionDialog({
  open,
  onOpenChange,
  position,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  position: Position;
}) {
  const { deletePosition, isPending } = useDeletePosition();

  const handleDelete = () => {
    deletePosition(
      { positionId: position.id },
      {
        onSuccess: () => {
          toast.success("Должность удалена");
          onOpenChange(false);
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Удалить должность</DialogTitle>
          <DialogDescription>
            Вы уверены, что хотите удалить должность &quot;
            <strong>{position.name}</strong>
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
