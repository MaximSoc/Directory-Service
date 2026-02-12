"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import LocationCard from "@/entities/locations/ui/location.card";
import { useLocationsList } from "../model/use-locations-list";
import { useGetLocationFilter } from "../model/locations-filter-store";

export function LocationsList() {
  const filter = useGetLocationFilter();
  const { locations, isPending, error, isFetchingNextPage, cursorRef } =
    useLocationsList(filter);

  if (error) {
    return <div className="text-destructive">Ошибка: {error.message}</div>;
  }

  if (isPending) {
    return (
      <div className="col-span-full flex justify-center p-10">
        <Spinner />
      </div>
    );
  }

  if (locations?.length === 0) {
    return (
      <div className="mt-10 text-center text-muted-foreground">
        Список локаций пуст.
      </div>
    );
  }

  return (
    <>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {locations?.map((location) => (
          <LocationCard key={location.id} location={location} />
        ))}
      </div>

      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </>
  );
}
