"use client";

import { usePosition } from "../model/use-position";
import { UpdatePositionDialog } from "../update-position-dialog";
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
import { Building2, Edit, FileText } from "lucide-react";
import { useState } from "react";

interface PositionDetailsProps {
  id: string;
}

export function PositionDetails({ id }: PositionDetailsProps) {
  const [openUpdate, setOpenUpdate] = useState(false);
  const { data, isLoading, isError } = usePosition(id);

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
          Ошибка загрузки данных
        </h2>
        <p className="text-muted-foreground">
          Должность не найдена или была удалена.
        </p>
      </div>
    );
  }

  const { position } = data;
  const departments = position.departments || [];

  return (
    <>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between mb-8">
        <div className="space-y-1">
          <h1 className="text-3xl font-bold tracking-tight">{position.name}</h1>
          <div className="flex items-center gap-3 text-sm text-muted-foreground">
            <Badge variant={position.isActive ? "default" : "secondary"}>
              {position.isActive ? "Активна" : "Неактивна"}
            </Badge>
            <span className="font-mono text-xs">ID: {position.id}</span>
          </div>
        </div>

        <Button onClick={() => setOpenUpdate(true)} className="shrink-0">
          <Edit className="mr-2 h-4 w-4" /> Редактировать
        </Button>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="md:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5 text-primary" />
                Описание
              </CardTitle>
            </CardHeader>
            <CardContent>
              {position.description ? (
                <p className="whitespace-pre-wrap text-muted-foreground">
                  {position.description}
                </p>
              ) : (
                <div className="text-center py-6 bg-muted/20 rounded-lg border border-dashed">
                  <p className="text-muted-foreground">Описание отсутствует</p>
                  <Button
                    variant="link"
                    size="sm"
                    onClick={() => setOpenUpdate(true)}
                  >
                    Добавить
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Building2 className="h-5 w-5 text-primary" /> Подразделения
                <Badge variant="secondary" className="ml-2">
                  {departments.length}
                </Badge>
              </CardTitle>
              <CardDescription>
                Связанные с должностью подразделения
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {departments.length > 0 ? (
                  departments.map((dep) => (
                    <Badge
                      key={dep.id}
                      variant="outline"
                      className="px-3 py-1.5"
                    >
                      {dep.name}
                    </Badge>
                  ))
                ) : (
                  <p className="text-muted-foreground text-sm">
                    Нет привязанных подразделений
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      <UpdatePositionDialog
        open={openUpdate}
        onOpenChange={setOpenUpdate}
        position={position}
      />
    </>
  );
}
