import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { EnvelopeError } from "@/shared/api/errors";
import { UpdateDepartmentFormData } from "../update-department-dialog";
import {
  departmentsApi,
  departmentsQueryOptions,
} from "@/entities/departments/api";

export function useUpdateDepartment() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({
      departmentId,
      ...data
    }: UpdateDepartmentFormData & { departmentId: string }) =>
      departmentsApi.updateDepartment({ departmentId, ...data }),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при изменении подразделения");
    },
    onSuccess: () => {
      toast.success("Подразделение успешно изменено");
    },
  });

  return {
    updateDepartment: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
