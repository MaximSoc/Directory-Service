import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { toast } from "sonner";
import { useDeleteDepartment } from "./use-delete-department";
import { Department } from "@/entities/departments/types";

export function DeleteDepartmentDialog({
  open,
  onOpenChange,
  department,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  department: Department;
}) {
  const { deleteDepartment, isPending } = useDeleteDepartment();

  const handleDelete = () => {
    deleteDepartment(
      { departmentId: department.id },
      {
        onSuccess: () => {
          toast.success("Подразделение удалено");
          onOpenChange(false);
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Удалить подразделение</DialogTitle>
          <DialogDescription>
            Вы уверены, что хотите удалить подразделение &quot;
            <strong>{department.name}</strong>
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
