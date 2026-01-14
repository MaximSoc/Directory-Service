import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Building2, MapPin, Users } from "lucide-react";

export default function Home() {
  return (
    <div className="min-h-screen bg-background">
      <section className="container mx-auto flex flex-col items-center justify-center gap-6 py-24 text-center md:py-32">
        <h1 className="text-4xl font-extrabold tracking-tight lg:text-5xl max-w-3xl">
          Единый источник правды для{" "}
          <span className="text-primary">оргструктуры компании</span>
        </h1>

        <p className="max-w-[42rem] leading-normal text-muted-foreground sm:text-xl sm:leading-8">
          Directory Service хранит базовые справочники — подразделения,
          должности и локации — и предоставляет единый CRUD‑интерфейс для всех
          внутренних сервисов (HR, Склад, Заказы).
        </p>
      </section>

      <section className="container mx-auto py-8 md:py-12 lg:py-24">
        <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
          <Card>
            <CardHeader>
              <Building2 className="h-10 w-10 text-primary mb-2" />
              <CardTitle>Подразделения</CardTitle>
              <CardDescription>
                Иерархическая структура департаментов и отделов.
              </CardDescription>
            </CardHeader>
          </Card>

          <Card>
            <CardHeader>
              <Users className="h-10 w-10 text-primary mb-2" />
              <CardTitle>Должности</CardTitle>
              <CardDescription>
                Реестр всех позиций и грейдов в компании.
              </CardDescription>
            </CardHeader>
          </Card>

          <Card>
            <CardHeader>
              <MapPin className="h-10 w-10 text-primary mb-2" />
              <CardTitle>Локации</CardTitle>
              <CardDescription>Офисы, склады, магазины и т.д.</CardDescription>
            </CardHeader>
          </Card>
        </div>
      </section>
    </div>
  );
}
