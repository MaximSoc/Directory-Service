export type SortDirection = "asc" | "desc";

export type PaginationResponse<T> = {
  items: T[];
  totalPages: number;
  page: number;
  pageSize?: number;
  totalCount?: number;
};
