import {
  departmentsQueryOptions,
  GetDepartmentChildrenRequest,
} from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentChildren(
  departmentId: string,
  params?: GetDepartmentChildrenRequest
) {
  return useQuery(
    departmentsQueryOptions.getDepartmentChildrenQueryOptions(
      departmentId,
      params
    )
  );
}
