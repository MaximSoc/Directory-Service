import { departmentsApi } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartment(id: string) {
  return useQuery({
    queryKey: ["departments", id],
    queryFn: () => departmentsApi.getDepartmentById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}
