import { locationsQueryOptions } from "@/entities/locations/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";
import { LocationsFilterState } from "./locations-filter-store";

export const PAGE_SIZE = 3;

export function useLocationsList({
  search,
  isActive,
  pageSize,
  sortBy,
  sortDirection,
}: LocationsFilterState) {
  const {
    data,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...locationsQueryOptions.getLessonsInfiniteOptions({
      search,
      isActive,
      pageSize,
      sortBy,
      sortDirection,
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
    locations: data?.locations,
    totalPages: data?.totalPages,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
    cursorRef,
  };
}
