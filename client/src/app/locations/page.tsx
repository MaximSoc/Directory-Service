"use client";

import React, { useState } from "react";
import { locationsApi } from "@/entities/locations/api";
import { Spinner } from "@/shared/components/ui/spinner";
import LocationCard from "@/features/locations/location.card";
import { useQuery } from "@tanstack/react-query";
import { PaginationCustom } from "@/features/pagination/pagination.custom";

export default function LocationsPage() {
  const [page, setPage] = useState(1);
  const pageSize = 1;

  const { data, isPending, error } = useQuery({
    queryFn: () =>
      locationsApi.getLocations({ page: page, pageSize: pageSize }),
    queryKey: ["locations", page],
  });

  const totalPages = data?.totalPages || 1;
  const locations = data?.locations || [];

  if (isPending) {
    return <Spinner />;
  }
  if (error) {
    return <div>Ошибка: {error.message}</div>;
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

        <PaginationCustom
          currentPage={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      </div>
    </main>
  );
}
