import { useQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import { useGetLocationFilter } from "./locations-filter-store";
import {
  GetLocationsRequest,
  locationsQueryOptions,
} from "@/entities/locations/api";

export function useQueryLocationsList(params?: GetLocationsRequest) {
  const globalFilters = useGetLocationFilter();

  const search =
    params?.search !== undefined ? params.search : globalFilters.search;
  const isActive =
    params?.isActive !== undefined ? params.isActive : globalFilters.isActive;

  const [debouncedSearch] = useDebounce(search, 300);

  return useQuery({
    ...locationsQueryOptions.getLocationsQueryOptions({
      search: debouncedSearch,
      isActive,
    }),
    select: (data) => data.items,
  });
}
