"use client";

import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { LocationsFilters } from "../locations-filters";
import { CreateLocationDialog } from "../create-location-dialog";

export function LocationsToolbar() {
  const [openCreate, setOpenCreate] = useState(false);

  return (
    <div className="mb-8 flex flex-col gap-4 items-start">
      <div className="w-full">
        <LocationsFilters />
      </div>

      <Button onClick={() => setOpenCreate(true)}>Создать локацию</Button>

      <CreateLocationDialog open={openCreate} onOpenChange={setOpenCreate} />
    </div>
  );
}
