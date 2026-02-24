"use client";

import { DepartmentDetails } from "@/features/departments/ui/department-details";
import { Button } from "@/shared/components/ui/button";
import { routes } from "@/shared/routes";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { use } from "react";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default function DepartmentDetailsPage({ params }: PageProps) {
  const resolvedParams = use(params);
  const id = resolvedParams.id;

  return (
    <div className="container mx-auto max-w-5xl py-8 px-4 sm:px-6">
      <div className="mb-6">
        <Button
          variant="ghost"
          className="-ml-4 text-muted-foreground hover:text-foreground"
          asChild
        >
          <Link href={routes.departments} className="flex items-center gap-2">
            <ArrowLeft className="h-4 w-4" /> Назад к списку
          </Link>
        </Button>
      </div>

      <DepartmentDetails id={id} />
    </div>
  );
}
