import { Loader2, XCircle } from "lucide-react";
import { Progress } from "@/shared/components/ui/progress";
import { Button } from "@/shared/components/ui/button";

type Props = {
  fileName?: string;
  progress: number;
  onAbort: () => void;
};

export function UploadingState({ fileName, progress, onAbort }: Props) {
  return (
    <div className="flex flex-col items-center justify-center space-y-6 p-8 border-2 border-dashed rounded-xl bg-muted/20">
      <div className="flex flex-col items-center gap-2">
        <Loader2 className="h-10 w-10 animate-spin text-primary" />
        <span className="text-sm font-medium animate-pulse">Загружаем...</span>
      </div>

      <div className="w-full space-y-4">
        <div className="space-y-2">
          <div className="flex justify-between text-[10px] font-mono text-muted-foreground">
            <span className="truncate max-w-50">{fileName || "Файл"}</span>
            <span>{progress}%</span>
          </div>
          <Progress value={progress} className="h-2.5 transition-all" />
        </div>

        <Button
          variant="ghost"
          size="sm"
          className="w-full text-muted-foreground hover:text-destructive gap-2"
          onClick={onAbort}
        >
          <XCircle className="h-4 w-4" />
          Отменить загрузку
        </Button>
      </div>
    </div>
  );
}
