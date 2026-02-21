"use client";

import {
  Folder,
  FileText,
  ChevronRight,
  ChevronDown,
  Loader2,
  MoreHorizontal,
} from "lucide-react";
import { useDepartmentTreeStore } from "../model/use-department-tree-store";
import { cn } from "@/shared/lib/utils";
import { Button } from "@/shared/components/ui/button";
import { Department } from "@/entities/departments/types";
import { useDepartmentChildrenInfinite } from "../model/use-department-tree-queries";
import Link from "next/link";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";

interface TreeNodeProps {
  node: Department;
}

export function TreeNode({ node }: TreeNodeProps) {
  const { expandedNodes, toggleNode } = useDepartmentTreeStore();
  const isExpanded = !!expandedNodes[node.id]?.isExpanded;

  const {
    children,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
    isLoading,
  } = useDepartmentChildrenInfinite(node.id, isExpanded);

  const canExpand = node.hasMoreChildren;

  return (
    <div className="flex flex-col">
      <div
        className={cn(
          "group flex items-center py-2 px-2 rounded-md hover:bg-accent/50 cursor-pointer transition-all",
          !node.isActive && "opacity-60"
        )}
        style={{ paddingLeft: `${node.depth * 24}px` }}
        onClick={() => canExpand && toggleNode(node.id)}
      >
        <div
          className="w-6 h-6 flex items-center justify-center mr-1 cursor-pointer hover:bg-accent rounded"
          onClick={(e) => {
            e.stopPropagation();
            if (canExpand) toggleNode(node.id);
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

        <div className="mr-2 text-muted-foreground">
          {canExpand ? (
            <Folder
              className={cn(
                "h-4 w-4 transition-colors",
                isExpanded && "text-blue-500 fill-blue-500/10"
              )}
            />
          ) : (
            <FileText className="h-4 w-4" />
          )}
        </div>

        <TooltipProvider delayDuration={400}>
          <Tooltip>
            <TooltipTrigger asChild>
              <Link
                href={`/departments/${node.id}`}
                className="flex items-center gap-2 text-sm font-medium hover:text-primary hover:underline decoration-primary/30 underline-offset-4 transition-colors"
              >
                {node.name}
              </Link>
            </TooltipTrigger>
            <TooltipContent
              side="top"
              align="start"
              className="max-w-xs break-all"
            >
              <p className="text-xs font-mono text-muted-foreground">Путь:</p>
              <p className="text-xs">{node.path}</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>

        {canExpand && !isExpanded && !isLoading && (
          <span className="ml-auto text-[10px] text-primary opacity-0 group-hover:opacity-100 transition-opacity font-semibold uppercase tracking-wider">
            Раскрыть
          </span>
        )}
      </div>

      {isExpanded && (
        <div className="flex flex-col animate-in fade-in slide-in-from-top-1 duration-200">
          {children.map((child) => (
            <TreeNode key={child.id} node={child} />
          ))}

          {hasNextPage && (
            <Button
              variant="ghost"
              size="sm"
              className="h-8 my-1 justify-start text-[11px] text-primary hover:bg-primary/5 font-medium"
              style={{ marginLeft: `${(node.depth + 1) * 24 + 24}px` }}
              onClick={(e) => {
                e.stopPropagation();
                fetchNextPage();
              }}
              disabled={isFetchingNextPage}
            >
              {isFetchingNextPage ? (
                <Loader2 className="h-3 w-3 animate-spin mr-2" />
              ) : (
                <MoreHorizontal className="h-3 w-3 mr-2" />
              )}
              Загрузить ещё подразделения...
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
