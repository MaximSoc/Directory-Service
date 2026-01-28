import { Location } from "@/entities/locations/types";
import { DeleteLocationDialog } from "@/features/locations/delete-location-dialog";
import { UpdateLocationDialog } from "@/features/locations/update-location-dialog";
import StatusBadge from "@/features/status/status.badge";
import { Button } from "@/shared/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import { Edit, Trash2, MoreHorizontal } from "lucide-react";
import { useState } from "react";

export default function LocationCard({ location }: { location: Location }) {
  const [openUpdate, setOpenUpdate] = useState(false);
  const [openDelete, setOpenDelete] = useState(false);

  return (
    <div className="flex flex-col justify-between rounded-xl border border-border bg-card p-6 text-card-foreground shadow-sm transition-colors hover:bg-accent/5">
      <div>
        <div className="mb-4 flex items-start justify-between">
          <h3
            className="text-lg font-semibold tracking-tight line-clamp-1"
            title={location.name}
          >
            {location.name}
          </h3>
          <div className="flex items-center gap-2">
            <StatusBadge isActive={location.isActive} />
            {/* Кнопки действий */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-8 w-8 p-0" size="sm">
                  <span className="sr-only">Открыть меню</span>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem
                  className="cursor-pointer gap-2"
                  onClick={() => setOpenUpdate(true)}
                >
                  <Edit className="h-4 w-4" />
                  Редактировать
                </DropdownMenuItem>
                <DropdownMenuItem
                  className="cursor-pointer gap-2 text-destructive focus:text-destructive"
                  onClick={() => setOpenDelete(true)}
                >
                  <Trash2 className="h-4 w-4" />
                  Удалить
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>

        <div className="space-y-1 text-sm text-muted-foreground">
          <p className="flex items-center gap-2">
            <span className="font-medium">Город:</span>
            <span className="text-foreground">{location.city}</span>
          </p>
          <p className="flex items-center gap-2">
            <span className="font-medium">Адрес:</span>
            <span className="text-foreground">
              {location.street}, д. {location.apartamentNumber}
            </span>
          </p>
        </div>
      </div>

      <div className="mt-6 border-t border-border pt-4">
        <button className="w-full cursor-pointer rounded-lg bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground transition-colors hover:bg-secondary/80 active:opacity-90">
          Подробнее
        </button>
      </div>

      <UpdateLocationDialog
        location={location}
        open={openUpdate}
        onOpenChange={setOpenUpdate}
      />
      <DeleteLocationDialog
        open={openDelete}
        onOpenChange={setOpenDelete}
        location={location}
      />
    </div>
  );
}
