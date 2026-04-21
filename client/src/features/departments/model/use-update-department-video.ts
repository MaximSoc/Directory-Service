import {
  departmentsApi,
  departmentsQueryOptions,
} from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

type UpdateDepartmentVideoParams = {
  departmentId: string;
  videoId?: string;
};

export function useUpdateDepartmentVideo() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({ departmentId, videoId }: UpdateDepartmentVideoParams) =>
      departmentsApi.updateDpartmentVideo(departmentId, videoId),
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.getFirstMessage());
        return;
      }

      toast.error("Ошибка при обновлении подразделения");
    },
    onSuccess: () => {
      toast.success("Медиа успешно приклреплено к подразделению");
    },
  });

  return {
    updateDepartmentVideo: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
