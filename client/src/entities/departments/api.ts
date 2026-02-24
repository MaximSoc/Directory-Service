import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { Department } from "./types";
import { DepartmentsFilterState } from "@/features/departments/model/departments-filter-store";
import { PaginationResponse } from "@/shared/types/custom-types";
import { PAGINATION_CONFIG } from "@/shared/constants/constants";

export type GetDepartmentsRequest = {
  search?: string;
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  parentId?: string;
  locationIds?: string[];
};

export type GetDepartmentChildrenRequest = {
  page?: number;
  pageSize?: number;
};

export type CreateDepartmentRequest = {
  name: string;
  identifier: string;
  locationIds: string[];
  parentId?: string;
};

export type UpdateDepartmentRequest = {
  name: string;
  identifier: string;
};

export type UpdateDepartmentLocationsRequest = {
  locationIds: string[];
};

export type GetDepartmentRootsRequest = {
  page?: number;
  pageSize?: number;
};

export const departmentsApi = {
  getDepartments: async (
    request: GetDepartmentsRequest
  ): Promise<PaginationResponse<Department>> => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Department>>
    >("/departments", { params: request });

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load departments");
    }

    return response.data.result!;
  },

  getDepartmentById: async (id: string): Promise<Department> => {
    const response = await apiClient.get<Envelope<Department>>(
      `/departments/${id}`
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load department details");
    }

    return response.data.result;
  },

  getDepartmentRoots: async (
    request: GetDepartmentRootsRequest
  ): Promise<PaginationResponse<Department>> => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Department>>
    >("/departments/roots", { params: request });
    return response.data.result!;
  },

  getDepartmentChildren: async ({
    departmentId,
    ...data
  }: {
    departmentId: string;
  } & GetDepartmentChildrenRequest): Promise<
    PaginationResponse<Department>
  > => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Department>>
    >(`/departments/${departmentId}/children`, { params: data });

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load children");
    }

    return response.data.result!;
  },

  createDepartment: async (
    request: CreateDepartmentRequest
  ): Promise<Envelope<Department>> => {
    const response = await apiClient.post<Envelope<Department>>(
      "/departments",
      request
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to create department");
    }

    return response.data;
  },

  updateDepartment: async ({
    departmentId,
    ...data
  }: { departmentId: string } & UpdateDepartmentRequest): Promise<
    Envelope<Department>
  > => {
    const response = await apiClient.put<Envelope<Department>>(
      `/departments/${departmentId}`,
      data
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to update department");
    }

    return response.data;
  },

  moveDepartment: async (
    departmentId: string,
    parentId?: string | null
  ): Promise<Envelope<void>> => {
    const response = await apiClient.put<Envelope<void>>(
      `/departments/${departmentId}/parent`,
      { departmentId, parentId }
    );
    return response.data;
  },

  updateDepartmentLocations: async (
    departmentId: string,
    request: UpdateDepartmentLocationsRequest
  ): Promise<Envelope<Department>> => {
    const response = await apiClient.put<Envelope<Department>>(
      `/departments/${departmentId}/locations`,
      request
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to update department locations");
    }

    return response.data;
  },

  deleteDepartment: async (
    departmentId: string
  ): Promise<Envelope<Department>> => {
    const response = await apiClient.delete<Envelope<Department>>(
      `/departments/${departmentId}`
    );

    return response.data;
  },
};

export const departmentsQueryOptions = {
  baseKey: "departments",

  getDepartmentsQueryOptions: (request: GetDepartmentsRequest) => {
    return queryOptions({
      queryFn: () =>
        departmentsApi.getDepartments({
          page: PAGINATION_CONFIG.DEFAULT.INITIAL_PAGE,
          pageSize: PAGINATION_CONFIG.DEPARTMENTS.MAX_PREVIEW,
          ...request,
        }),
      queryKey: [departmentsQueryOptions.baseKey, request],
    });
  },

  getDepartmentsInfiniteOptions: (filter: DepartmentsFilterState) => {
    return infiniteQueryOptions({
      queryKey: [departmentsQueryOptions.baseKey, filter],
      queryFn: ({ pageParam }) => {
        return departmentsApi.getDepartments({ ...filter, page: pageParam });
      },
      initialPageParam: PAGINATION_CONFIG.DEFAULT.INITIAL_PAGE,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },

      select: (data): PaginationResponse<Department> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalPages: data.pages[0]?.totalPages ?? 0,
        page: data.pages[0]?.page ?? 1,
        totalCount: data.pages[0]?.totalCount ?? 0,
      }),
    });
  },

  getDepartmentChildrenQueryOptions: (
    departmentId: string,
    request: GetDepartmentChildrenRequest = {
      page: PAGINATION_CONFIG.DEFAULT.INITIAL_PAGE,
      pageSize: PAGINATION_CONFIG.DEPARTMENTS.MAX_PREVIEW,
    }
  ) => {
    return queryOptions({
      queryFn: () =>
        departmentsApi.getDepartmentChildren({ departmentId, ...request }),
      queryKey: [
        departmentsQueryOptions.baseKey,
        "children",
        departmentId,
        request,
      ],
    });
  },

  getTreeRootsInfiniteOptions: (isActive?: boolean) => {
    return infiniteQueryOptions({
      queryKey: [
        departmentsQueryOptions.baseKey,
        "tree",
        "roots",
        { isActive },
      ],
      queryFn: ({ pageParam }) => {
        return departmentsApi.getDepartmentRoots({
          page: pageParam,
          pageSize: PAGINATION_CONFIG.DEPARTMENTS.TREE_ROOTS_SIZE,
        });
      },
      initialPageParam: PAGINATION_CONFIG.DEFAULT.INITIAL_PAGE,
      getNextPageParam: (lastPage) => {
        if (lastPage.page >= lastPage.totalPages) return undefined;
        return lastPage.page + 1;
      },
    });
  },

  getDepartmentChildrenInfiniteOptions: (departmentId: string) => {
    return infiniteQueryOptions({
      queryKey: [
        departmentsQueryOptions.baseKey,
        "tree",
        "children",
        departmentId,
      ],
      queryFn: ({ pageParam }) => {
        return departmentsApi.getDepartmentChildren({
          departmentId,
          page: pageParam,
          pageSize: PAGINATION_CONFIG.DEPARTMENTS.TREE_CHILDREN_SIZE,
        });
      },
      initialPageParam: PAGINATION_CONFIG.DEFAULT.INITIAL_PAGE,
      getNextPageParam: (lastPage) => {
        if (lastPage.page >= lastPage.totalPages) return undefined;
        return lastPage.page + 1;
      },
    });
  },
};
