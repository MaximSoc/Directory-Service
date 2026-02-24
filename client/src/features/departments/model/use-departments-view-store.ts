import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type ViewMode = "list" | "tree";

interface DepartmentsViewStore {
  viewMode: ViewMode;
  setViewMode: (mode: ViewMode) => void;
}

export const useDepartmentsViewStore = create<DepartmentsViewStore>()(
  persist(
    (set) => ({
      viewMode: "list",
      setViewMode: (mode) => set({ viewMode: mode }),
    }),
    {
      name: "departments-view-storage",
      storage: createJSONStorage(() => localStorage),
    }
  )
);
