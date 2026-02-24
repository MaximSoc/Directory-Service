"use client";

import DepartmentCard from "@/entities/departments/ui/department.card";
import { Spinner } from "@/shared/components/ui/spinner";
import { useInfiniteQueryDepartmentsList } from "../model/use-infinite-query-departments-list";

export function DepartmentsList() {
  const { departments, error, isFetchingNextPage, cursorRef } =
    useInfiniteQueryDepartmentsList();

  if (error)
    return <div className="p-4 text-destructive">Ошибка: {error.message}</div>;

  if (departments?.length === 0) {
    return (
      <div className="mt-10 text-center text-muted-foreground">
        Список пуст.
      </div>
    );
  }

  return (
    <>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {departments?.map((department) => (
          <DepartmentCard key={department.id} department={department} />
        ))}
      </div>

      <div ref={cursorRef} className="flex justify-center py-6">
        {isFetchingNextPage && <Spinner />}
      </div>
    </>
  );
}
