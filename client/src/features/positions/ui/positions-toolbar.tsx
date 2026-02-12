import { Button } from "@/shared/components/ui/button";
import { PositionsFilters } from "../position-filters";
import { CreatePositionDialog } from "../create-position-dialog";
import { useState } from "react";

export function PositionsToolbar() {
  const [openCreate, setOpenCreate] = useState(false);

  return (
    <div className="mb-8 flex flex-col gap-4 items-start">
      <div className="w-full">
        <PositionsFilters />
      </div>

      <div className="flex shrink-0 items-center">
        <Button onClick={() => setOpenCreate(true)}>Создать должность</Button>
      </div>

      <CreatePositionDialog open={openCreate} onOpenChange={setOpenCreate} />
    </div>
  );
}
