import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { Department } from "./types";
import { DepartmentsFilterState } from "@/features/departments/model/departments-filter-store";

export type GetDepartmentsResponse = {
  items: Department[];
  totalPages: number;
  page: number;
  totalCount: number;
};

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
  parentId?: string;
};

export type UpdateDepartmentLocationsRequest = {
  locationIds: string[];
};

export type GetOneDepartmentResponse = {
  department: Department;
};

export type GetRootsRequest = {
  page?: number;
  pageSize?: number;
};

export const departmentsApi = {
  getDepartments: async (
    request: GetDepartmentsRequest
  ): Promise<GetDepartmentsResponse> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments",
      { params: request }
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load departments");
    }

    return response.data.result!;
  },

  getOneDepartment: async (id: string): Promise<GetOneDepartmentResponse> => {
    const response = await apiClient.get<Envelope<GetOneDepartmentResponse>>(
      `/departments/${id}`
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load department details");
    }

    return response.data.result;
  },

  getRoots: async (
    request: GetRootsRequest
  ): Promise<GetDepartmentsResponse> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments/roots",
      { params: request }
    );
    return response.data.result!;
  },

  getDepartmentChildren: async ({
    departmentId,
    ...data
  }: {
    departmentId: string;
  } & GetDepartmentChildrenRequest): Promise<GetDepartmentsResponse> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      `/departments/${departmentId}/children`,
      { params: data }
    );

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
        departmentsApi.getDepartments({ page: 1, pageSize: 1000, ...request }),
      queryKey: [departmentsQueryOptions.baseKey, request],
    });
  },

  getDepartmentsInfiniteOptions: (filter: DepartmentsFilterState) => {
    return infiniteQueryOptions({
      queryKey: [departmentsQueryOptions.baseKey, filter],
      queryFn: ({ pageParam }) => {
        return departmentsApi.getDepartments({ ...filter, page: pageParam });
      },
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },

      select: (data): GetDepartmentsResponse => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalPages: data.pages[0]?.totalPages ?? 0,
        page: data.pages[0]?.page ?? 1,
        totalCount: data.pages[0]?.totalCount ?? 0,
      }),
    });
  },

  getDepartmentChildrenQueryOptions: (
    departmentId: string,
    request: GetDepartmentChildrenRequest = { page: 1, pageSize: 10 }
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
        return departmentsApi.getRoots({
          page: pageParam,
          pageSize: 10,
        });
      },
      initialPageParam: 1,
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
          pageSize: 10,
        });
      },
      initialPageParam: 1,
      getNextPageParam: (lastPage) => {
        if (lastPage.page >= lastPage.totalPages) return undefined;
        return lastPage.page + 1;
      },
    });
  },
};
