import {
  departmentsQueryOptions,
  GetDepartmentsRequest,
} from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentsList(params?: GetDepartmentsRequest) {
  const search = params?.search;
  const isActive = params?.isActive;

  return useQuery({
    ...departmentsQueryOptions.getDeparmentsQueryOptions({
      search,
      isActive,
    }),
    select: (data) => data.departments,
  });
}
