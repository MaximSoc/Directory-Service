"use client";

import { queryClient } from "@/shared/api/query-client";
import { SidebarInset, SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import { AppSidebar } from "../sidebar/app.sidebar";
import Header from "../header/header";
import { Toaster } from "@/shared/components/ui/sonner";

export default function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <QueryClientProvider client={queryClient}>
      <SidebarProvider>
        <AppSidebar />
        <SidebarInset>
          <Header />
          <main className="flex-1 p-6 md:p-10">{children}</main>
          <Toaster position="top-center" duration={3000} richColors={true} />
        </SidebarInset>
      </SidebarProvider>
    </QueryClientProvider>
  );
}
