"use client";

import { usePositionsList } from "../model/use-positions-list";
import PositionCard from "@/entities/positions/ui/position.card";
import { Spinner } from "@/shared/components/ui/spinner";
import { useGetPositionFilter } from "../model/positions-filter-store";

export function PositionsList() {
  const filter = useGetPositionFilter();

  const { positions, error, isFetchingNextPage, cursorRef } =
    usePositionsList(filter);

  if (error) {
    return <div>Ошибка: {error.message}</div>;
  }

  if (positions?.length === 0) {
    return (
      <div className="mt-10 text-center text-muted-foreground">
        Список должностей пуст.
      </div>
    );
  }

  return (
    <>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {positions?.map((position) => (
          <PositionCard key={position.id} position={position} />
        ))}
      </div>

      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </>
  );
}
