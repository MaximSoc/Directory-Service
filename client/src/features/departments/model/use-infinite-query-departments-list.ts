import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import {
  DepartmentsFilterState,
  useGetDepartmentsFilter,
} from "./departments-filter-store";
import { departmentsQueryOptions } from "@/entities/departments/api";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";

export function useInfiniteQueryDepartmentsList(
  params?: Partial<DepartmentsFilterState>
) {
  const globalFilter = useGetDepartmentsFilter();

  const search = params?.search ?? globalFilter.search;
  const isActive = params?.isActive ?? globalFilter.isActive;
  const pageSize = params?.pageSize ?? globalFilter.pageSize;
  const sortBy = params?.sortBy ?? globalFilter.sortBy;
  const sortDirection = params?.sortDirection ?? globalFilter.sortDirection;
  const parentId = params?.parentId ?? globalFilter.parentId;
  const locationIds = params?.locationIds ?? globalFilter.locationIds;

  const [debouncedSearch] = useDebounce(search, 300);

  const {
    data,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentsInfiniteOptions({
      search: debouncedSearch,
      isActive,
      pageSize,
      sortBy,
      sortDirection,
      parentId,
      locationIds,
    }),
  });

  const cursorRef = useCursorRef({
    hasNextPage: !!hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    departments: data?.items,
    totalPages: data?.totalPages,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
    cursorRef,
  };
}
