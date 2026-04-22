import { CheckCircle2 } from "lucide-react";

export function CompleteState() {
  return (
    <div className="flex flex-col items-center justify-center space-y-4 p-8 border-2 border-dashed rounded-xl bg-green-500/5 border-green-500/20">
      <CheckCircle2 className="h-12 w-12 text-green-500" />
      <div className="text-center">
        <p className="font-medium text-green-600">Загрузка завершена</p>
        <p className="text-xs text-green-600/70">
          Файл успешно обработан и сохранен
        </p>
      </div>
    </div>
  );
}
