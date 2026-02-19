"use client";

import { Button } from "@/shared/components/ui/button";
import { routes } from "@/shared/routes";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { PositionDetails } from "@/features/positions/ui/position-details";

export default function PositionDetailsPage() {
  const params = useParams();
  const id = params.id as string;

  return (
    <div className="container mx-auto max-w-5xl py-8 px-4 sm:px-6">
      <div className="mb-6">
        <Button
          variant="ghost"
          className="-ml-4 text-muted-foreground hover:text-foreground"
          asChild
        >
          <Link href={routes.positions} className="flex items-center gap-2">
            <ArrowLeft className="h-4 w-4" /> Назад к списку
          </Link>
        </Button>
      </div>

      <PositionDetails id={id} />
    </div>
  );
}
