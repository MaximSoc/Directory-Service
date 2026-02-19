import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/shared/components/ui/dialog";
import { Button } from "@/shared/components/ui/button";
import { LocationsMultiSelect } from "../locations/model/locations-multi-select";
import { useUpdateDepartmentLocations } from "./model/use-update-department-locations";

interface UpdateDepartmentLocationsProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  department: {
    id: string;
    name: string;
    locations: { id: string }[];
  };
}

export function UpdateDepartmentLocationsDialog({
  open,
  onOpenChange,
  department,
}: UpdateDepartmentLocationsProps) {
  const [selectedIds, setSelectedIds] = useState<string[]>(
    department.locations.map((l) => l.id)
  );

  const { updateDepartmentLocations, isPending } =
    useUpdateDepartmentLocations();

  const handleSave = () => {
    updateDepartmentLocations(
      {
        id: department.id,
        data: { locationIds: selectedIds },
      },
      {
        onSuccess: () => onOpenChange(false),
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-106.25">
        <DialogHeader>
          <DialogTitle>Редактирование локаций</DialogTitle>
          <DialogDescription>
            Укажите города или офисы, к которым относится{" "}
            <strong>{department.name}</strong>
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <LocationsMultiSelect value={selectedIds} onChange={setSelectedIds} />
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Отмена
          </Button>
          <Button onClick={handleSave} disabled={isPending}>
            Сохранить изменения
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
