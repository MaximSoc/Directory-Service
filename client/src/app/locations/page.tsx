"use client";

import React, { useEffect, useState } from "react";
import { Location } from "@/entities/locations/types";
import { locationsApi } from "@/entities/locations/api";
import { Spinner } from "@/shared/components/ui/spinner";
import LocationCard from "@/features/locations/location.card";

export default function LocationsPage() {
  const [locations, setLocations] = useState<Location[]>([]);

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    locationsApi
      .getLocations()
      .then((data) => setLocations(data))
      .finally(() => setIsLoading(false))
      .catch((error) => setError(error.message));
  }, []);

  if (isLoading) {
    return <Spinner />;
  }
  if (error) {
    return <div>Ошибка: {error}</div>;
  }

  return (
    <main className="min-h-screen bg-background p-8 text-foreground">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8">
          <h1 className="text-3xl font-bold tracking-tight">Локации</h1>
        </header>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {locations?.map((location) => (
            <LocationCard key={location.id} location={location} />
          ))}
        </div>

        {locations.length === 0 && (
          <div className="mt-10 text-center text-muted-foreground">
            Список локаций пуст.
          </div>
        )}
      </div>
    </main>
  );
}
