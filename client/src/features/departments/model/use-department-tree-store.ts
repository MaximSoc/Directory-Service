import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

interface DepartmentTreeState {
  expandedNodeIds: string[];
  toggleNode: (departmentId: string) => void;
  expandNode: (departmentId: string) => void;
  collapseNode: (departmentId: string) => void;
  resetTree: () => void;
}

export const useDepartmentTreeStore = create<DepartmentTreeState>()(
  persist(
    (set) => ({
      expandedNodeIds: [],

      toggleNode: (id) =>
        set((state) => ({
          expandedNodeIds: state.expandedNodeIds.includes(id)
            ? state.expandedNodeIds.filter((nodeId) => nodeId !== id)
            : [...state.expandedNodeIds, id],
        })),

      expandNode: (id) =>
        set((state) => ({
          expandedNodeIds: state.expandedNodeIds.includes(id)
            ? state.expandedNodeIds
            : [...state.expandedNodeIds, id],
        })),

      collapseNode: (id) =>
        set((state) => ({
          expandedNodeIds: state.expandedNodeIds.filter(
            (nodeId) => nodeId !== id
          ),
        })),

      resetTree: () => set({ expandedNodeIds: [] }),
    }),
    {
      name: "department-tree-storage",
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);
