import {
  departmentsQueryOptions,
  GetDepartmentsRequest,
} from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";
import { useGetDepartmentsFilter } from "./departments-filter-store";
import { useDebounce } from "use-debounce";

export function useQueryDepartmentsList(params?: GetDepartmentsRequest) {
  const globalFilters = useGetDepartmentsFilter();

  const search =
    params?.search !== undefined ? params.search : globalFilters.search;
  const isActive =
    params?.isActive !== undefined ? params.isActive : globalFilters.isActive;

  const [debouncedSearch] = useDebounce(search, 300);

  return useQuery({
    ...departmentsQueryOptions.getDepartmentsQueryOptions({
      search: debouncedSearch,
      isActive,
    }),
    select: (data) => data.items,
  });
}
