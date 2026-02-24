import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import {
  PositionsFilterState,
  useGetPositionFilter,
} from "./positions-filter-store";
import { positionsQueryOptions } from "@/entities/positions/api";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";

export function usePositionsList(params?: Partial<PositionsFilterState>) {
  const globalFilter = useGetPositionFilter();

  const search = params?.search ?? globalFilter.search;
  const isActive = params?.isActive ?? globalFilter.isActive;
  const pageSize = params?.pageSize ?? globalFilter.pageSize;
  const sortBy = params?.sortBy ?? globalFilter.sortBy;
  const sortDirection = params?.sortDirection ?? globalFilter.sortDirection;
  const departmentIds = params?.departmentIds ?? globalFilter.departmentIds;

  const [debouncedSearch] = useDebounce(search, 300);

  const {
    data,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...positionsQueryOptions.getPositionsInfiniteOptions({
      search: debouncedSearch,
      isActive,
      pageSize,
      sortBy,
      sortDirection,
      departmentIds,
    }),
  });

  const cursorRef = useCursorRef({
    hasNextPage: !!hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    positions: data?.items,
    totalPages: data?.totalPages,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
    cursorRef,
  };
}
