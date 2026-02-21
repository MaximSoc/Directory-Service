import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

interface NodeMetadata {
  isExpanded: boolean;
}

interface DepartmentTreeState {
  expandedNodes: Record<string, NodeMetadata>;
  toggleNode: (departmentId: string) => void;
  expandNode: (departmentId: string) => void;
  collapseNode: (departmentId: string) => void;
  resetTree: () => void;
}

export const useDepartmentTreeStore = create<DepartmentTreeState>()(
  persist(
    (set) => ({
      expandedNodes: {},

      toggleNode: (id) =>
        set((state) => ({
          expandedNodes: {
            ...state.expandedNodes,
            [id]: {
              ...state.expandedNodes[id],
              isExpanded: !state.expandedNodes[id]?.isExpanded,
            },
          },
        })),

      expandNode: (id) =>
        set((state) => ({
          expandedNodes: {
            ...state.expandedNodes,
            [id]: { ...state.expandedNodes[id], isExpanded: true },
          },
        })),

      collapseNode: (id) =>
        set((state) => ({
          expandedNodes: {
            ...state.expandedNodes,
            [id]: { ...state.expandedNodes[id], isExpanded: false },
          },
        })),

      resetTree: () => set({ expandedNodes: {} }),
    }),
    {
      name: "department-tree-storage",
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);
