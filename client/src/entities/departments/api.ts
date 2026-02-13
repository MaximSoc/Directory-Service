import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { queryOptions } from "@tanstack/react-query";
import { Department } from "./types";

export type GetDepartmentsResponse = {
  departments: Department[];
};

export type GetDepartmentsRequest = {
  search?: string;
  isActive?: boolean;
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

    return response.data.result;
  },
};

export const departmentsQueryOptions = {
  baseKey: "departments",

  getDeparmentsQueryOptions: ({
    search,
    isActive,
  }: {
    search?: string;
    isActive?: boolean;
  }) => {
    return queryOptions({
      queryFn: () =>
        departmentsApi.getDepartments({ search: search, isActive: isActive }),
      queryKey: [departmentsQueryOptions.baseKey, { search, isActive }],
    });
  },
};
