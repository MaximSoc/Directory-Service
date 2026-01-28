import { queryOptions } from "@tanstack/react-query";
import { Location } from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";

type GetLocationsByDepartmentResponse = {
  locations: Location[];
  totalPages: number;
};

export type GetLocationsByDepartmentRequest = {
  search?: string;
  page: number;
  pageSize: number;
};

export type CreateLocationRequest = {
  name: string;
  country: string;
  region: string;
  city: string;
  postalCode: string;
  street: string;
  apartamentNumber: string;
  timezone: string;
};

export type UpdateLocationRequest = {
  name: string;
  country: string;
  region: string;
  city: string;
  postalCode: string;
  street: string;
  apartamentNumber: string;
  timezone: string;
};

export const locationsApi = {
  getLocations: async (
    request: GetLocationsByDepartmentRequest
  ): Promise<GetLocationsByDepartmentResponse> => {
    const response = await apiClient.get<
      Envelope<GetLocationsByDepartmentResponse>
    >("/locations", { params: request });

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load locations");
    }

    return response.data.result;
  },

  createLocation: async (
    request: CreateLocationRequest
  ): Promise<Envelope<Location>> => {
    const response = await apiClient.post<Envelope<Location>>(
      "/locations",
      request
    );

    return response.data;
  },

  updateLocation: async ({
    id,
    ...data
  }: { id: string } & UpdateLocationRequest): Promise<Envelope<Location>> => {
    const response = await apiClient.put<Envelope<Location>>(
      `/locations/${id}`,
      data
    );

    return response.data;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",

  getLocationsOptions: ({
    page,
    pageSize,
  }: {
    page: number;
    pageSize: number;
  }) => {
    return queryOptions({
      queryFn: () =>
        locationsApi.getLocations({ page: page, pageSize: pageSize }),
      queryKey: [locationsQueryOptions.baseKey, { page }],
    });
  },
};
