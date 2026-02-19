import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { Location } from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { LocationsFilterState } from "@/features/locations/model/locations-filter-store";

export type GetLocationsResponse = {
  items: Location[];
  totalPages: number;
  page: number;
};

export type GetLocationsRequest = {
  search?: string;
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  departmentIds?: string[];
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
    request: GetLocationsRequest
  ): Promise<GetLocationsResponse> => {
    const response = await apiClient.get<Envelope<GetLocationsResponse>>(
      "/locations",
      { params: request }
    );

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

  getLocationsQueryOptions: (request: GetLocationsRequest) => {
    return queryOptions({
      queryFn: () =>
        locationsApi.getLocations({ page: 1, pageSize: 1000, ...request }),
      queryKey: [locationsQueryOptions.baseKey, request],
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

      select: (data): GetLocationsResponse => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalPages: data.pages[0]?.totalPages ?? 0,
        page: data.pages[0]?.page ?? 1,
      }),
    });
  },
};
