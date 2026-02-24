import { departmentsQueryOptions } from "@/entities/departments/api";
import { useInfiniteQuery } from "@tanstack/react-query";

export const useDepartmentRootsInfinite = (isActive?: boolean) => {
  const query = useInfiniteQuery(
    departmentsQueryOptions.getTreeRootsInfiniteOptions(isActive)
  );

  return {
    roots: query.data?.pages.flatMap((page) => page.items) ?? [],
    hasNextPage: query.hasNextPage,
    isFetchingNextPage: query.isFetchingNextPage,
    fetchNextPage: query.fetchNextPage,
    isLoading: query.isLoading,
    error: query.error,
  };
};

export const useDepartmentChildrenInfinite = (
  departmentId: string,
  enabled: boolean
) => {
  const query = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentChildrenInfiniteOptions(
      departmentId
    ),
    enabled,
  });

  return {
    children: query.data?.pages.flatMap((page) => page.items) ?? [],
    hasNextPage: query.hasNextPage,
    isFetchingNextPage: query.isFetchingNextPage,
    fetchNextPage: query.fetchNextPage,
    isLoading: query.isLoading,
    error: query.error,
  };
};
