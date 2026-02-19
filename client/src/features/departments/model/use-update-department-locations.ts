import {
  departmentsApi,
  departmentsQueryOptions,
  UpdateDepartmentLocationsRequest,
} from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateDepartmentLocations() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: UpdateDepartmentLocationsRequest;
    }) => departmentsApi.updateDepartmentLocations(id, data),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при изменении связанных локаций");
    },
    onSuccess: () => {
      toast.success("Локации успешно изменены");
    },
  });

  return {
    updateDepartmentLocations: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
