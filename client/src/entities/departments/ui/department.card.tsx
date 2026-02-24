"use client";

import StatusBadge from "@/features/status/status.badge";
import { Button } from "@/shared/components/ui/button";
import { Trash2, Building2, ChevronRight } from "lucide-react";
import { useState } from "react";
import Link from "next/link";
import { Department } from "../types"; // Предполагаем наличие типа
import { DeleteDepartmentDialog } from "@/features/departments/model/delete-department-dialog";

export default function DepartmentCard({
  department,
}: {
  department: Department;
}) {
  const [openDelete, setOpenDelete] = useState(false);

  return (
    <div className="flex flex-col justify-between rounded-xl border border-border bg-card p-6 text-card-foreground shadow-sm transition-all hover:shadow-md hover:bg-accent/5">
      <div>
        <div className="mb-4 flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-primary/10 rounded-lg text-primary">
              <Building2 className="h-5 w-5" />
            </div>
            <div>
              <h3
                className="text-lg font-semibold tracking-tight line-clamp-1"
                title={department.name}
              >
                {department.name}
              </h3>
              <span className="text-xs font-mono text-muted-foreground uppercase">
                {department.identifier}
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <StatusBadge isActive={department.isActive} />
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive hover:bg-destructive hover:text-destructive-foreground"
              onClick={() => setOpenDelete(true)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <div className="space-y-3 text-sm">
          <div className="bg-muted/50 p-2 rounded text-xs font-mono text-muted-foreground break-all">
            {department.path}
          </div>

          <div className="space-y-1 text-muted-foreground">
            <p className="flex items-center gap-2">
              <span className="font-medium">Уровень:</span>
              <span className="text-foreground">{department.depth}</span>
            </p>
          </div>
        </div>
      </div>

      <div className="mt-6 border-t border-border pt-4">
        <Button asChild className="w-full justify-between" variant="secondary">
          <Link href={`/departments/${department.id}`}>
            Подробнее
            <ChevronRight className="h-4 w-4" />
          </Link>
        </Button>
      </div>

      <DeleteDepartmentDialog
        open={openDelete}
        onOpenChange={setOpenDelete}
        department={department}
      />
    </div>
  );
}
