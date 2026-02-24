"use client";

import { Folder, FileText, Loader2, MoreHorizontal } from "lucide-react";
import { useDepartmentTreeStore } from "../model/use-department-tree-store";
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
import { TreeItem } from "@/shared/components/ui/tree-item";

interface DepartmentTreeNodeProps {
  node: Department;
}

export function DepartmentTreeNode({ node }: DepartmentTreeNodeProps) {
  const isExpanded = useDepartmentTreeStore((state) =>
    state.expandedNodeIds.includes(node.id)
  );
  const toggleNode = useDepartmentTreeStore((state) => state.toggleNode);

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
      <TreeItem
        level={node.depth}
        isExpanded={isExpanded}
        canExpand={canExpand}
        isLoading={isLoading}
        isActive={node.isActive}
        onToggle={() => toggleNode(node.id)}
        icon={
          canExpand ? (
            <Folder
              className={isExpanded ? "text-blue-500 fill-blue-500/10" : ""}
            />
          ) : (
            <FileText />
          )
        }
        label={
          <TooltipProvider delayDuration={400}>
            <Tooltip>
              <TooltipTrigger asChild>
                <Link
                  href={`/departments/${node.id}`}
                  className="text-sm font-medium hover:text-primary hover:underline underline-offset-4 transition-colors truncate block"
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
        }
        actions={
          canExpand &&
          !isExpanded &&
          !isLoading && (
            <span
              className="text-[10px] text-primary opacity-0 group-hover:opacity-100 transition-opacity font-semibold uppercase tracking-wider px-2"
              onClick={(e) => {
                e.stopPropagation();
                toggleNode(node.id);
              }}
            >
              Раскрыть
            </span>
          )
        }
      />

      {isExpanded && (
        <div className="flex flex-col animate-in fade-in slide-in-from-top-1 duration-200">
          {children.map((child) => (
            <DepartmentTreeNode key={child.id} node={child} />
          ))}

          {hasNextPage && (
            <Button
              variant="ghost"
              size="sm"
              className="h-8 my-1 justify-start text-[11px] text-primary hover:bg-primary/5 font-medium"
              style={{ marginLeft: `${(node.depth + 1) * 24 + 12}px` }}
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
              Загрузить ещё...
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
