import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import {
  CreateLocationRequest,
  GetLocationsByDepartmentRequest,
  GetLocationsByDepartmentResponse,
  Location,
  UpdateLocationRequest,
} from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { LocationsFilterState } from "@/features/locations/model/locations-filter-store";

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

  deleteLocation: async (locationId: string): Promise<Envelope<Location>> => {
    const response = await apiClient.delete<Envelope<Location>>(
      `/locations/${locationId}`
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

  getLocationsInfiniteOptions: (filter: LocationsFilterState) => {
    return infiniteQueryOptions({
      queryKey: [locationsQueryOptions.baseKey, filter],
      queryFn: ({ pageParam }) => {
        return locationsApi.getLocations({ ...filter, page: pageParam });
      },
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },

      select: (data): GetLocationsByDepartmentResponse => ({
        locations: data.pages.flatMap((page) => page?.locations ?? []),
        totalPages: data.pages[0]?.totalPages ?? 0,
        page: data.pages[0]?.page ?? 1,
      }),
    });
  },
};
