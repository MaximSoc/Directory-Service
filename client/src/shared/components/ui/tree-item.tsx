import { ReactNode } from "react";
import { ChevronDown, ChevronRight, Loader2 } from "lucide-react";
import { cn } from "@/shared/lib/utils";

interface TreeItemProps {
  label: ReactNode;
  icon?: ReactNode;
  level: number;
  isExpanded?: boolean;
  canExpand?: boolean;
  isLoading?: boolean;
  isActive?: boolean;
  onToggle: () => void;
  actions?: ReactNode;
}

export function TreeItem({
  label,
  icon,
  level,
  isExpanded,
  canExpand,
  isLoading,
  isActive = true,
  onToggle,
  actions,
}: TreeItemProps) {
  return (
    <div
      className={cn(
        "group flex items-center py-2 px-2 rounded-md hover:bg-accent/50 cursor-pointer transition-all",
        !isActive && "opacity-60"
      )}
      style={{ paddingLeft: `${level * 24}px` }}
    >
      <div
        className="w-6 h-6 flex items-center justify-center mr-1 cursor-pointer hover:bg-accent rounded"
        onClick={(e) => {
          e.stopPropagation();
          if (canExpand) onToggle();
        }}
      >
        {isLoading ? (
          <Loader2 className="h-3 w-3 animate-spin text-muted-foreground" />
        ) : canExpand ? (
          isExpanded ? (
            <ChevronDown className="h-4 w-4 text-primary" />
          ) : (
            <ChevronRight className="h-4 w-4 text-muted-foreground" />
          )
        ) : (
          <div className="w-1 h-1 bg-muted-foreground/30 rounded-full" />
        )}
      </div>

      {icon && <div className="mr-2 text-muted-foreground">{icon}</div>}

      <div className="flex-1 overflow-hidden">{label}</div>

      {actions && <div className="ml-auto">{actions}</div>}
    </div>
  );
}
