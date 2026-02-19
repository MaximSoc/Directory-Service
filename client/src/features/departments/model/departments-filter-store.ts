import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { SortDirection } from "@/shared/types/custom-types";
import { PAGE_SIZE_DEPARTMENTS } from "@/shared/constants/constants";

export type DepartmentSortField = "name" | "path" | "createdAt";

export type DepartmentsFilterState = {
  search: string;
  isActive?: boolean;
  pageSize: number;
  sortBy: DepartmentSortField;
  sortDirection: SortDirection;
  parentId?: string;
  locationIds: string[];
};

type Actions = {
  setSearch: (input: DepartmentsFilterState["search"]) => void;
  setIsActive: (isActive: DepartmentsFilterState["isActive"]) => void;
  setSortBy: (sortBy: DepartmentsFilterState["sortBy"]) => void;
  setDirection: (
    sortDirection: DepartmentsFilterState["sortDirection"]
  ) => void;
  setParentId: (parentId: DepartmentsFilterState["parentId"]) => void;
  setLocationIds: (locationIds: DepartmentsFilterState["locationIds"]) => void;
  reset: () => void;
};

type DepartmentsFilterStore = DepartmentsFilterState & Actions;

const initialState: DepartmentsFilterState = {
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE_DEPARTMENTS,
  sortBy: "name",
  sortDirection: "asc",
  parentId: undefined,
  locationIds: [],
};

export const useDepartmentsFilterStore = create<DepartmentsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setSearch: (input: DepartmentsFilterState["search"]) =>
        set({ search: input }),
      setIsActive: (isActive: DepartmentsFilterState["isActive"]) =>
        set({ isActive }),
      setSortBy: (sortBy: DepartmentsFilterState["sortBy"]) =>
        set(() => ({ sortBy })),
      setDirection: (sortDirection: DepartmentsFilterState["sortDirection"]) =>
        set(() => ({ sortDirection })),
      setParentId: (parentId: DepartmentsFilterState["parentId"]) =>
        set(() => ({ parentId })),
      setLocationIds: (locationIds: DepartmentsFilterState["locationIds"]) =>
        set(() => ({ locationIds })),
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
      pageSize: state.pageSize,
      sortBy: state.sortBy,
      sortDirection: state.sortDirection,
      parentId: state.parentId,
      locationIds: state.locationIds,
    }))
  );
};

export const setDepartmentsFilterSearch = (
  input: DepartmentsFilterState["search"]
) => useDepartmentsFilterStore.getState().setSearch(input);

export const setDepartmentsFilterIsActive = (
  input: DepartmentsFilterState["isActive"]
) => useDepartmentsFilterStore.getState().setIsActive(input);

export const setDepartmentsFilterSortBy = (
  input: DepartmentsFilterState["sortBy"]
) => useDepartmentsFilterStore.getState().setSortBy(input);

export const setDepartmentsFilterSortDirection = (
  input: DepartmentsFilterState["sortDirection"]
) => useDepartmentsFilterStore.getState().setDirection(input);

export const setDepartmentsFilterParentId = (
  input: DepartmentsFilterState["parentId"]
) => useDepartmentsFilterStore.getState().setParentId(input);

export const setDepartmentsFilterByLocationIds = (
  input: DepartmentsFilterState["locationIds"]
) => useDepartmentsFilterStore.getState().setLocationIds(input);
