import {
  departmentsApi,
  GetDepartmentsRequest,
} from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentsList(params?: GetDepartmentsRequest) {
  return useQuery({
    queryKey: ["departments", params],
    queryFn: () =>
      departmentsApi
        .getDepartments(params ?? {})
        .then((data) => data.departments),
    staleTime: 5 * 60 * 1000,
  });
}
