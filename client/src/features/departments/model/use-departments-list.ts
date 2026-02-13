import {
  departmentsQueryOptions,
  GetDepartmentsRequest,
} from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";
import { useGetDepartmentsFilter } from "./departments-filter-store";
import { useDebounce } from "use-debounce";

export function useDepartmentsList(params?: GetDepartmentsRequest) {
  const globalFilters = useGetDepartmentsFilter();

  const search = params?.search ?? globalFilters.search;
  const isActive = params?.isActive ?? globalFilters.isActive;

  const [debouncedSearch] = useDebounce(search, 300);

  return useQuery({
    ...departmentsQueryOptions.getDeparmentsQueryOptions({
      search: debouncedSearch,
      isActive,
    }),
    select: (data) => data.departments,
  });
}
