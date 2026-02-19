import { locationsQueryOptions } from "@/entities/locations/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";
import {
  LocationsFilterState,
  useGetLocationFilter,
} from "./locations-filter-store";
import { useDebounce } from "use-debounce";

export function useInfiniteQueryLocationsList(
  params?: Partial<LocationsFilterState>
) {
  const globalFilter = useGetLocationFilter();

  const search = params?.search ?? globalFilter.search;
  const isActive = params?.isActive ?? globalFilter.isActive;
  const pageSize = params?.pageSize ?? globalFilter.pageSize;
  const sortBy = params?.sortBy ?? globalFilter.sortBy;
  const sortDirection = params?.sortDirection ?? globalFilter.sortDirection;

  const [debouncedSearch] = useDebounce(search, 300);

  const {
    data,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...locationsQueryOptions.getLocationsInfiniteOptions({
      search: debouncedSearch,
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
    locations: data?.items,
    totalPages: data?.totalPages,
    isPending,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
    cursorRef,
  };
}
