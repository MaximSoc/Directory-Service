"use client";

import { Building2, Home, MapPin, Briefcase } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "@/components/ui/sidebar";
import { routes } from "@/shared/routes";

// Конфигурация меню с иконками
const items = [
  {
    title: "Главная",
    url: routes.home,
    icon: Home,
  },
  {
    title: "Подразделения",
    url: routes.departments,
    icon: Building2,
  },
  {
    title: "Локации",
    url: routes.locations,
    icon: MapPin,
  },
  {
    title: "Должности",
    url: routes.positions,
    icon: Briefcase,
  },
];

export function AppSidebar() {
  const pathname = usePathname();

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader className="flex items-center justify-center py-4">
        {/* Можно добавить логотип или название, которое будет скрываться при сворачивании */}
        <span className="font-bold text-xl group-data-[collapsible=icon]:hidden">
          Меню
        </span>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Навигация</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {items.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton
                    asChild
                    isActive={pathname === item.url}
                    tooltip={item.title} // Всплывающая подсказка при свернутом меню
                  >
                    <Link href={item.url}>
                      <item.icon />
                      <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarRail />
    </Sidebar>
  );
}
