import { Location } from "./types";
import { apiClient } from "@/shared/api/axios-instance";

type GetLocationsByDepartmentResponse = {
  locations: Location[];
};

export const locationsApi = {
  getLocations: async (): Promise<Location[]> => {
    const response = await apiClient.get<GetLocationsByDepartmentResponse>(
      "/locations"
    );

    return response.data.locations;
  },
};
