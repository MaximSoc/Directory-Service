import { Location } from "./types";
import { apiClient } from "@/shared/api/axios-instance";

type GetLocationsByDepartmentResponse = {
  locations: Location[];
  totalPages: number;
};

export type GetLocationsByDepartmentRequest = {
  search?: string;
  page: number;
  pageSize: number;
};

export const locationsApi = {
  getLocations: async (
    request: GetLocationsByDepartmentRequest
  ): Promise<GetLocationsByDepartmentResponse> => {
    const response = await apiClient.get<GetLocationsByDepartmentResponse>(
      "/locations",
      { params: request }
    );

    return response.data;
  },
};
