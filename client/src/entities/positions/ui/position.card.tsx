import StatusBadge from "@/features/status/status.badge";
import { Button } from "@/shared/components/ui/button";
import { Trash2 } from "lucide-react";
import { useState } from "react";
import { Position } from "../types";
import { DeletePositionDialog } from "@/features/positions/delete-position-dialog";
import Link from "next/link";

export default function PositionCard({ position }: { position: Position }) {
  //   const [openUpdate, setOpenUpdate] = useState(false);
  const [openDelete, setOpenDelete] = useState(false);

  return (
    <div className="flex flex-col justify-between rounded-xl border border-border bg-card p-6 text-card-foreground shadow-sm transition-colors hover:bg-accent/5">
      <div>
        <div className="mb-4 flex items-start justify-between">
          <h3
            className="text-lg font-semibold tracking-tight line-clamp-1"
            title={position.name}
          >
            {position.name}
          </h3>
          <div className="flex items-center gap-2">
            <StatusBadge isActive={position.isActive} />
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive hover:bg-destructive hover:text-destructive-foreground transition-colors"
              onClick={() => setOpenDelete(true)}
              title="Удалить должность"
            >
              <Trash2 className="h-4 w-4" />
              <span className="sr-only">Удалить</span>
            </Button>
          </div>
        </div>

        <div className="space-y-1 text-sm text-muted-foreground">
          <p className="flex items-center gap-2">
            <span className="font-medium">Описание:</span>
            <span className="text-foreground">{position.description}</span>
          </p>
          <p className="flex items-center gap-2">
            <span className="font-medium">Количество подразделений:</span>
            <span className="text-foreground">{position.departmentCount}</span>
          </p>
        </div>
      </div>

      <div className="mt-6 border-t border-border pt-4">
        <Button asChild className="w-full" variant="secondary">
          <Link href={`/positions/${position.id}`}>Подробнее</Link>
        </Button>
      </div>

      <DeletePositionDialog
        open={openDelete}
        onOpenChange={setOpenDelete}
        position={position}
      />
    </div>
  );
}
