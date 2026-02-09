import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { UpdateLocationFormData } from "../update-location-dialog";

export function useUpdateLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({ id, ...data }: UpdateLocationFormData & { id: string }) =>
      locationsApi.updateLocation({ id, ...data }),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      }),
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при изменении локации");
    },
    onSuccess: () => {
      toast.success("Локация успешно изменена");
    },
  });

  return {
    updateLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
