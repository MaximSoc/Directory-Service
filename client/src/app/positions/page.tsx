"use client";

import { PositionsList } from "@/features/positions/ui/positions-list";
import { PositionsToolbar } from "@/features/positions/ui/positions-toolbar";

export default function PositionsPage() {
  return (
    <main className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8">
          <h1 className="text-3xl font-bold">Должности</h1>
        </header>

        <PositionsToolbar />

        <PositionsList />
      </div>
    </main>
  );
}
