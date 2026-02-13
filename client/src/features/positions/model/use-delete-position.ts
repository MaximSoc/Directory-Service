import { positionsApi, positionsQueryOptions } from "@/entities/positions/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeletePosition() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({ positionId }: { positionId: string }) =>
      positionsApi.deletePosition(positionId),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [positionsQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при удалении должности");
    },
    onSuccess: () => {
      toast.success("Должность удалена");
    },
  });

  return {
    deletePosition: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
