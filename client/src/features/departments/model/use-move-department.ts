import {
  departmentsApi,
  departmentsQueryOptions,
} from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useMoveDepartment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      departmentId,
      parentId,
    }: {
      departmentId: string;
      parentId?: string | null;
    }) => departmentsApi.moveDepartment(departmentId, parentId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при изменении родительского подразделения");
    },
  });
}
