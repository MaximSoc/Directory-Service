"use client";

import { Button } from "@/shared/components/ui/button";
import { AlertCircle } from "lucide-react";
import { useDepartmentRootsInfinite } from "../model/use-department-tree-queries";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { DepartmentTreeNode } from "./department-tree-node";

interface DepartmentsTreeProps {
  isActive?: boolean;
}

export function DepartmentsTree({ isActive }: DepartmentsTreeProps) {
  const { roots, hasNextPage, fetchNextPage, isLoading, error } =
    useDepartmentRootsInfinite(isActive);

  if (isLoading && roots.length === 0) {
    return (
      <div className="flex flex-col gap-3 p-4 bg-card border rounded-lg">
        {[...Array(6)].map((_, i) => (
          <Skeleton key={i} className="h-6 w-full" />
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center gap-2 p-4 text-destructive bg-destructive/5 rounded-lg border border-destructive/20">
        <AlertCircle className="h-5 w-5" />
        <span className="text-sm">
          Ошибка при загрузке структуры подразделений
        </span>
      </div>
    );
  }

  return (
    <div className="rounded-xl border bg-card shadow-sm p-2">
      <div className="flex flex-col gap-1">
        {roots.map((node) => (
          <DepartmentTreeNode key={node.id} node={node} />
        ))}
      </div>

      {hasNextPage && (
        <div className="mt-4 border-t pt-4 flex justify-center">
          <Button variant="outline" onClick={() => fetchNextPage()}>
            Показать больше корневых подразделений
          </Button>
        </div>
      )}
    </div>
  );
}
