import { apiClient } from "@/shared/api/axios-instance";
import {
  CreatePositionRequest,
  GetOnePositionResponse,
  GetPositionsRequest,
  GetPositionsResponse,
  Position,
  UpdatePositionRequest,
} from "./types";
import { Envelope } from "@/shared/api/envelope";
import { infiniteQueryOptions } from "@tanstack/react-query";
import { PositionsFilterState } from "@/features/positions/model/positions-filter-store";

export const positionsApi = {
  getPositions: async (
    request: GetPositionsRequest
  ): Promise<GetPositionsResponse> => {
    const response = await apiClient.get<Envelope<GetPositionsResponse>>(
      "/positions",
      { params: request }
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load positions");
    }

    return response.data.result;
  },

  createPosition: async (
    request: CreatePositionRequest
  ): Promise<Envelope<Position>> => {
    const response = await apiClient.post<Envelope<Position>>(
      "/positions",
      request
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to create position");
    }

    return response.data;
  },

  deletePosition: async (positionId: string): Promise<Envelope<Position>> => {
    const response = await apiClient.delete<Envelope<Position>>(
      `/positions/${positionId}`
    );

    return response.data;
  },

  getOnePosition: async (id: string): Promise<GetOnePositionResponse> => {
    const response = await apiClient.get<Envelope<GetOnePositionResponse>>(
      `/positions/${id}`
    );

    if (response.data.isError || !response.data.result) {
      throw new Error("Failed to load position details");
    }

    return response.data.result;
  },

  updatePosition: async ({
    id,
    ...data
  }: { id: string } & UpdatePositionRequest): Promise<Envelope<Position>> => {
    const response = await apiClient.put<Envelope<Position>>(
      `/positions/${id}`,
      data
    );

    if (response.data.isError) {
      throw new Error("Failed to update position");
    }

    return response.data;
  },
};

export const positionsQueryOptions = {
  baseKey: "positions",

  getPositionsInfiniteOptions: (filter: PositionsFilterState) => {
    return infiniteQueryOptions({
      queryKey: [positionsQueryOptions.baseKey, filter],
      queryFn: ({ pageParam }) => {
        return positionsApi.getPositions({ ...filter, page: pageParam });
      },
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },

      select: (data): GetPositionsResponse => ({
        positions: data.pages.flatMap((page) => page?.positions ?? []),
        totalPages: data.pages[0]?.totalPages ?? 0,
        page: data.pages[0]?.page ?? 1,
      }),
    });
  },
};
