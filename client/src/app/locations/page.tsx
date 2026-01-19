"use client";

import React, { useEffect, useState } from "react";
import { Location } from "@/entities/locations/types";
import { locationsApi } from "@/entities/locations/api";
import { Spinner } from "@/shared/components/ui/spinner";

const LocationCard = ({ location }: { location: Location }) => {
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
          <StatusBadge isActive={location.isActive} />
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
    </div>
  );
};

const StatusBadge = ({ isActive }: { isActive: boolean }) => {
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 ${
        isActive
          ? "border-transparent bg-green-500/15 text-green-700 dark:text-green-400"
          : "border-transparent bg-destructive/15 text-destructive"
      }`}
    >
      {isActive ? "Активен" : "Неактивен"}
    </span>
  );
};

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
