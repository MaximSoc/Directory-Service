import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";

export type DepartmentsFilterState = {
  search: string;
  isActive?: boolean;
};

type Actions = {
  setSearch: (input: DepartmentsFilterState["search"]) => void;
  setIsActive: (isActive: DepartmentsFilterState["isActive"]) => void;
  reset: () => void;
};

type DepartmentsFilterStore = DepartmentsFilterState & Actions;

const initialState: DepartmentsFilterState = {
  search: "",
  isActive: true,
};

export const useDepartmentsFilterStore = create<DepartmentsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setSearch: (input) => set({ search: input }),
      setIsActive: (isActive) => set({ isActive }),
      reset: () => set(initialState),
    }),
    {
      name: "departments-filter",
      storage: createJSONStorage(() => localStorage),
    }
  )
);

export const useGetDepartmentsFilter = () => {
  return useDepartmentsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
    }))
  );
};

export const setDepartmentsSearch = (input: string) =>
  useDepartmentsFilterStore.getState().setSearch(input);

export const setDepartmentsIsActive = (isActive?: boolean) =>
  useDepartmentsFilterStore.getState().setIsActive(isActive);
