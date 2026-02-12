import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";
import { useDebounce } from "use-debounce";
import { PositionsFilterState } from "./positions-filter-store";
import { positionsQueryOptions } from "@/entities/positions/api";

export function usePositionsList({
  search,
  isActive,
  pageSize,
  sortBy,
  sortDirection,
  departmentIds,
}: PositionsFilterState) {
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

  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        {
          threshold: 0.5,
        }
      );

      if (el) {
        observer.observe(el);

        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  );

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
