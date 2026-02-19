"use client";

import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Spinner } from "@/shared/components/ui/spinner";
import {
  Edit,
  MapPin,
  Briefcase,
  ChevronRight,
  Hash,
  Layers,
  GitGraph,
  ChevronLeft,
} from "lucide-react";
import { Fragment, useState } from "react";
import Link from "next/link";
import { cn } from "@/shared/lib/utils";
import { useDepartment } from "@/features/locations/model/use-department";
import { UpdateDepartmentLocationsDialog } from "../update-department-locations-dialog";
import { UpdateDepartmentDialog } from "../update-department-dialog";
import { useDepartmentChildren } from "../model/use-query-department-children";

interface DepartmentDetailsProps {
  id: string;
}

export function DepartmentDetails({ id }: DepartmentDetailsProps) {
  const [openUpdate, setOpenUpdate] = useState(false);
  const [openLocationsUpdate, setOpenLocationsUpdate] = useState(false);
  const { data, isLoading, isError } = useDepartment(id);

  const [currentChildrenPage, setCurrentChildrenPage] = useState(1);
  const childrenPageSize = 5;

  const { data: childrenData, isLoading: isChildrenLoading } =
    useDepartmentChildren(id, {
      page: currentChildrenPage,
      pageSize: childrenPageSize,
    });

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="h-10 w-10" />
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="text-center py-20">
        <h2 className="text-xl font-semibold text-destructive">
          Ошибка загрузки
        </h2>
        <p className="text-muted-foreground">Подразделение не найдено.</p>
      </div>
    );
  }

  const { department } = data;

  const pathSegments = department.path.split(".").filter(Boolean);

  return (
    <>
      <nav className="flex items-center gap-1 text-sm text-muted-foreground mb-6 overflow-x-auto pb-2">
        <Link
          href="/departments"
          className="hover:text-primary transition-colors"
        >
          Подразделения
        </Link>
        {pathSegments.map((segment, index) => (
          <Fragment key={index}>
            <ChevronRight className="h-4 w-4 shrink-0" />
            <span
              className={cn(
                "whitespace-nowrap",
                index === pathSegments.length - 1 &&
                  "font-semibold text-foreground"
              )}
            >
              {segment}
            </span>
          </Fragment>
        ))}
      </nav>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between mb-8">
        <div className="space-y-1">
          <div className="flex items-center gap-3">
            <h1 className="text-3xl font-bold tracking-tight">
              {department.name}
            </h1>
            <Badge variant={department.isActive ? "default" : "secondary"}>
              {department.isActive ? "Активно" : "Неактивно"}
            </Badge>
          </div>
          <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
            <span className="flex items-center gap-1 font-mono text-xs">
              <Hash className="h-3 w-3" /> {department.identifier}
            </span>
            <span className="flex items-center gap-1">
              <Layers className="h-3 w-3" /> Уровень вложенности:{" "}
              {department.depth}
            </span>
          </div>
        </div>

        <Button onClick={() => setOpenUpdate(true)} className="shrink-0">
          <Edit className="mr-2 h-4 w-4" /> Редактировать
        </Button>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="md:col-span-2 space-y-6">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="flex items-center gap-2 text-lg">
                <GitGraph className="h-5 w-5 text-primary" />
                Дочерние подразделения
                <Badge variant="secondary" className="ml-auto">
                  {childrenData?.totalCount ?? 0}
                </Badge>
              </CardTitle>

              {/* Мини-навигация в заголовке, если страниц много */}
              {(childrenData?.totalPages ?? 0) > 1 && (
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() =>
                      setCurrentChildrenPage((p) => Math.max(1, p - 1))
                    }
                    disabled={currentChildrenPage === 1 || isChildrenLoading}
                  >
                    <ChevronLeft className="h-4 w-4" />
                  </Button>
                  <span className="text-xs font-medium">
                    {currentChildrenPage} / {childrenData?.totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() => setCurrentChildrenPage((p) => p + 1)}
                    disabled={
                      currentChildrenPage >= (childrenData?.totalPages ?? 1) ||
                      isChildrenLoading
                    }
                  >
                    <ChevronRight className="h-4 w-4" />
                  </Button>
                </div>
              )}
            </CardHeader>
            <CardContent>
              <div
                className={cn(
                  "grid gap-3 transition-opacity",
                  isChildrenLoading && "opacity-50" // Визуальный фидбек при смене страницы
                )}
              >
                {childrenData?.items && childrenData.items.length > 0
                  ? childrenData.items.map((child) => (
                      <Link
                        key={child.id}
                        href={`/departments/${child.id}`}
                        className="group flex items-center justify-between p-4 rounded-lg border bg-card hover:bg-accent transition-all"
                      >
                        <div className="space-y-1">
                          <p className="font-medium">{child.name}</p>
                          <p className="text-xs text-muted-foreground font-mono">
                            {child.identifier}
                          </p>
                        </div>
                        <ChevronRight className="h-4 w-4 text-muted-foreground group-hover:translate-x-1 transition-transform" />
                      </Link>
                    ))
                  : !isChildrenLoading && (
                      <p className="text-muted-foreground text-sm py-6 italic text-center border rounded-lg border-dashed">
                        Нет вложенных подразделений
                      </p>
                    )}

                {isChildrenLoading && (
                  <div className="flex justify-center py-4">
                    <Spinner />
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-lg">
                <Briefcase className="h-5 w-5 text-primary" />
                Позиции в подразделении
                <Badge variant="secondary" className="ml-auto">
                  {department.positions?.length || 0}
                </Badge>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid gap-2 sm:grid-cols-2">
                {department.positions?.length > 0 ? (
                  department.positions.map((pos) => (
                    <div
                      key={pos.id}
                      className="flex items-center p-3 rounded-md border bg-muted/30 text-sm hover:bg-muted/50 transition-colors"
                    >
                      {pos.name}
                    </div>
                  ))
                ) : (
                  <p className="text-muted-foreground text-sm py-4 italic text-center col-span-2">
                    Позиции еще не добавлены
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0">
              <CardTitle className="flex items-center gap-2 text-lg">
                <MapPin className="h-5 w-5 text-primary" />
                Локации
              </CardTitle>
              <Button
                variant="ghost"
                size="sm"
                className="h-8 px-2 text-xs"
                onClick={() => setOpenLocationsUpdate(true)}
              >
                <Edit className="mr-1 h-3 w-3" /> Изменить
              </Button>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {department.locations?.length > 0 ? (
                  department.locations.map((loc) => (
                    <Badge
                      key={loc.id}
                      variant="outline"
                      className="px-3 py-1 text-sm font-normal"
                    >
                      {loc.name}
                    </Badge>
                  ))
                ) : (
                  <p className="text-muted-foreground text-sm italic text-center w-full py-2">
                    Локации не привязаны
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-sm font-medium uppercase text-muted-foreground">
                Иерархия
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <label className="text-xs text-muted-foreground">
                  Родительский ID
                </label>
                <p className="font-mono text-sm break-all">
                  {department.parentId || "— (Корневое)"}
                </p>
              </div>
              <div>
                <label className="text-xs text-muted-foreground">
                  Системный путь
                </label>
                <p className="font-mono text-xs bg-muted p-2 rounded mt-1 border">
                  {department.path}
                </p>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      <UpdateDepartmentLocationsDialog
        open={openLocationsUpdate}
        onOpenChange={setOpenLocationsUpdate}
        department={department}
      />

      <UpdateDepartmentDialog
        open={openUpdate}
        onOpenChange={setOpenUpdate}
        department={department}
      />
    </>
  );
}
