import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { infiniteQueryOptions } from "@tanstack/react-query";
import { PositionsFilterState } from "@/features/positions/model/positions-filter-store";
import { Position } from "./types";
import { PaginationResponse } from "@/shared/types/custom-types";
import { PAGINATION_CONFIG } from "@/shared/constants/constants";

export type GetPositionsRequest = {
  search?: string;
  page: number;
  pageSize: number;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
};

export type CreatePositionRequest = {
  name: string;
  description?: string | undefined;
  departmentsIds: string[];
};

export type GetOnePositionResponse = {
  position: Position;
};

export type UpdatePositionRequest = {
  name: string;
  description?: string | undefined;
  departmentsIds: string[];
};

export const positionsApi = {
  getPositions: async (
    request: GetPositionsRequest
  ): Promise<PaginationResponse<Position>> => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Position>>
    >("/positions", { params: request });

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
      initialPageParam: PAGINATION_CONFIG.DEFAULT.INITIAL_PAGE,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) return undefined;
        return response.page + 1;
      },

      select: (data): PaginationResponse<Position> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalPages: data.pages[0]?.totalPages ?? 0,
        page: data.pages[0]?.page ?? 1,
      }),
    });
  },
};
