import { apiClient } from "@/shared/api/axios-instance";
import { Department } from "./types";
import { Envelope } from "@/shared/api/envelope";

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
