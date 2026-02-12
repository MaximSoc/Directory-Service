"use client";

import { LocationsToolbar } from "@/features/locations/ui/locations-toolbar";
import { LocationsList } from "@/features/locations/ui/locations-list";

export default function LocationsPage() {
  return (
    <main className="min-h-screen bg-background p-8 text-foreground">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8">
          <h1 className="text-3xl font-bold tracking-tight">Локации</h1>
        </header>

        <LocationsToolbar />
        <LocationsList />
      </div>
    </main>
  );
}
