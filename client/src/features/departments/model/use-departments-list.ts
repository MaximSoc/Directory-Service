import { GetDepartmentsResponse } from "@/entities/departments/api";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentsList() {
  return useQuery({
    queryKey: ["departments"],
    queryFn: () =>
      apiClient
        .get<Envelope<GetDepartmentsResponse>>("/departments")
        .then((response) => response.data.result?.departments),
    staleTime: 5 * 60 * 1000,
  });
}
