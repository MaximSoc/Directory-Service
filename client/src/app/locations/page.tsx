"use client";

import React, { useState } from "react";
import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import LocationCard from "@/entities/locations/ui/location.card";
import { CreateLocationDialog } from "@/features/locations/create-location-dialog";
import { useLocationsList } from "@/features/locations/model/use-locations-list";
import { LocationsFilters } from "@/features/locations/locations-filters";
import { useGetLocationFilter } from "@/features/locations/model/locations-filter-store";

export default function LocationsPage() {
  const { search, isActive, pageSize, sortBy, sortDirection } =
    useGetLocationFilter();
  const [openCreate, setOpenCreate] = useState(false);

  const { locations, isPending, error, isFetchingNextPage, cursorRef } =
    useLocationsList({
      search,
      isActive,
      pageSize,
      sortBy,
      sortDirection,
    });

  if (error) {
    return <div>Ошибка: {error.message}</div>;
  }

  return (
    <main className="min-h-screen bg-background p-8 text-foreground">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8">
          <h1 className="text-3xl font-bold tracking-tight">Локации</h1>
        </header>

        <div className="mb-8 flex flex-col gap-4 items-start">
          <LocationsFilters />

          <Button onClick={() => setOpenCreate(true)}>Создать локацию</Button>
        </div>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {isPending ? (
            <div className="col-span-full flex justify-center p-10">
              <Spinner />
            </div>
          ) : (
            locations?.map((location) => (
              <LocationCard key={location.id} location={location} />
            ))
          )}
        </div>

        {locations?.length === 0 && !isPending && (
          <div className="mt-10 text-center text-muted-foreground">
            Список локаций пуст.
          </div>
        )}

        <CreateLocationDialog open={openCreate} onOpenChange={setOpenCreate} />

        <div ref={cursorRef} className="flex justify-center py-4">
          {isFetchingNextPage && <Spinner />}
        </div>
      </div>
    </main>
  );
}
