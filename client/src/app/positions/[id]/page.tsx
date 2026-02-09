"use client";

import { usePosition } from "@/features/positions/model/use-position";
import { UpdatePositionDialog } from "@/features/positions/update-position-dialog";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Spinner } from "@/shared/components/ui/spinner";
import { routes } from "@/shared/routes";
import { ArrowLeft, Building2, Edit, FileText } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";

export default function PositionDetailsPage() {
  const params = useParams();
  const id = params.id as string;

  const [openUpdate, setOpenUpdate] = useState(false);

  const { data, isLoading, isError } = usePosition(id);

  const departments = useMemo(() => {
    if (!data?.position) return [];

    const { departmentIds, departmentNames } = data.position;

    return departmentIds.map((id, index) => ({
      id,
      name: departmentNames[index] || "Неизвестно",
    }));
  }, [data]);

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Spinner className="h-10 w-10" />
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="container flex flex-col items-center justify-center py-20 text-center">
        <h1 className="text-2xl font-bold text-destructive">
          Ошибка загрузки должности
        </h1>
        <p className="text-muted-foreground mt-2">
          Не удалось получить данные о должности. Возможно, она была удалена.
        </p>
        <Button variant="outline" asChild className="mt-6">
          <Link href={routes.positions}>Вернуться к списку</Link>
        </Button>
      </div>
    );
  }

  const { position } = data;

  return (
    <div className="container mx-auto max-w-5xl py-8 px-4 sm:px-6">
      <div className="mb-6">
        <Button
          variant="ghost"
          className="-ml-4 text-muted-foreground hover:text-foreground"
          asChild
        >
          <Link href={routes.positions} className="flex items-center gap-2">
            <ArrowLeft className="h-4 w-4" />
            Назад к списку должностей
          </Link>
        </Button>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between mb-8">
        <div className="space-y-1">
          <h1 className="text-3xl font-bold tracking-tight text-foreground">
            {position.name}
          </h1>
          <div className="flex items-center gap-3 text-sm text-muted-foreground">
            <Badge
              variant={position.isActive ? "default" : "secondary"}
              className="rounded-sm px-2 font-normal"
            >
              {position.isActive ? "Активна" : "Неактивна"}
            </Badge>
            <span className="flex items-center gap-1">
              ID: <span className="font-mono text-xs">{position.id}</span>
            </span>
          </div>
        </div>

        <Button
          onClick={() => setOpenUpdate(true)}
          className="shrink-0 shadow-sm"
        >
          <Edit className="mr-2 h-4 w-4" />
          Редактировать
        </Button>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="md:col-span-2 space-y-6">
          <Card className="mb-6">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-xl">
                <FileText className="h-5 w-5 text-primary" />
                Описание
              </CardTitle>
            </CardHeader>
            <CardContent>
              {position.description ? (
                <p className="whitespace-pre-wrap text-muted-foreground leading-relaxed">
                  {position.description}
                </p>
              ) : (
                <div className="flex flex-col items-center justify-center py-8 text-center text-muted-foreground bg-muted/20 rounded-lg border border-dashed">
                  <p>Описание отсутствует</p>
                  <Button
                    variant="link"
                    size="sm"
                    onClick={() => setOpenUpdate(true)}
                    className="mt-1 h-auto p-0"
                  >
                    Добавить описание
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-xl">
                <Building2 className="h-5 w-5 text-primary" />
                Подразделения
                <Badge variant="secondary" className="ml-2 rounded-full">
                  {departments.length}
                </Badge>
              </CardTitle>
              <CardDescription>
                Список подразделений, к которым привязана должность
              </CardDescription>
            </CardHeader>
            <CardContent>
              {departments.length > 0 ? (
                <div className="flex flex-wrap gap-2">
                  {departments.map((dep) => (
                    <Badge
                      key={dep.id}
                      variant="outline"
                      className="px-3 py-1.5 text-sm hover:bg-accent transition-colors cursor-default"
                    >
                      {dep.name}
                    </Badge>
                  ))}
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center py-8 text-center text-muted-foreground bg-muted/20 rounded-lg border border-dashed">
                  <p>Нет привязанных подразделений</p>
                  <Button
                    variant="link"
                    size="sm"
                    onClick={() => setOpenUpdate(true)}
                    className="mt-1 h-auto p-0"
                  >
                    Привязать подразделения
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      <UpdatePositionDialog
        open={openUpdate}
        onOpenChange={setOpenUpdate}
        position={position}
      />
    </div>
  );
}
