import { AlertCircle } from "lucide-react";

type Props = {
  error?: string;
};

export function ErrorState({ error }: Props) {
  return (
    <div className="flex flex-col items-center justify-center space-y-4 p-8 border-2 border-dashed rounded-xl bg-destructive/5 border-destructive/20">
      <AlertCircle className="h-12 w-12 text-destructive" />
      <div className="text-center space-y-2">
        <p className="font-medium text-destructive">Ошибка загрузки</p>
        <p className="text-xs text-destructive bg-destructive/10 p-2 rounded leading-relaxed">
          {error || "Произошла неизвестная ошибка"}
        </p>
      </div>
    </div>
  );
}
