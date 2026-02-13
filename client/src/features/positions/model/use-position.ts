import { positionsApi } from "@/entities/positions/api";
import { useQuery } from "@tanstack/react-query";

export function usePosition(id: string) {
  return useQuery({
    queryKey: ["positions", id],
    queryFn: () => positionsApi.getOnePosition(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}
