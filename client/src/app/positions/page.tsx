"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import { useGetPositionFilter } from "@/features/positions/model/positions-filter-store";
import { usePositionsList } from "@/features/positions/model/use-positions-list";
import { PositionsFilters } from "@/features/positions/position-filters";
import PositionCard from "@/entities/positions/ui/position.card";
import { useState } from "react";
import { CreatePositionDialog } from "@/features/positions/create-position-dialog";

export default function PositionsPage() {
  const filter = useGetPositionFilter();
  const [openCreate, setOpenCreate] = useState(false);

  const { positions, isPending, error, isFetchingNextPage, cursorRef } =
    usePositionsList(filter);

  if (error) {
    return <div>Ошибка: {error.message}</div>;
  }

  return (
    <main className="min-h-screen bg-background p-8 text-foreground">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8">
          <h1 className="text-3xl font-bold tracking-tight">Должности</h1>
        </header>

        <div className="mb-8 flex flex-col gap-4 items-start">
          <PositionsFilters />

          <Button onClick={() => setOpenCreate(true)}>Создать должность</Button>
        </div>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {isPending ? (
            <div className="col-span-full flex justify-center p-10">
              <Spinner />
            </div>
          ) : (
            positions?.map((position) => (
              <PositionCard key={position.id} position={position} />
            ))
          )}
        </div>

        {positions?.length === 0 && !isPending && (
          <div className="mt-10 text-center text-muted-foreground">
            Список локаций пуст.
          </div>
        )}

        <CreatePositionDialog open={openCreate} onOpenChange={setOpenCreate} />

        <div ref={cursorRef} className="flex justify-center py-4">
          {isFetchingNextPage && <Spinner />}
        </div>
      </div>
    </main>
  );
}
