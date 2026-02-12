import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { SortDirection } from "@/shared/types/custom-types";
import { PAGE_SIZE_POSITIONS } from "@/shared/constants/constants";

export type PositionSortField = "name" | "isActive";

export type PositionsFilterState = {
  search: string;
  isActive?: boolean;
  pageSize: number;
  sortBy: PositionSortField;
  sortDirection: SortDirection;
  departmentIds: string[];
};

type Actions = {
  setSearch: (input: PositionsFilterState["search"]) => void;
  setIsActive: (isActive: PositionsFilterState["isActive"]) => void;
  setSortBy: (sortBy: PositionsFilterState["sortBy"]) => void;
  setDirection: (sortDirection: PositionsFilterState["sortDirection"]) => void;
  setDepartmentIds: (
    departmentIds: PositionsFilterState["departmentIds"]
  ) => void;
};

type PositionsFilterStore = PositionsFilterState & Actions;

const initialState: PositionsFilterState = {
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE_POSITIONS,
  sortBy: "name",
  sortDirection: "asc",
  departmentIds: [],
};

export const usePositionsFilterStore = create<PositionsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setSearch: (input: PositionsFilterState["search"]) =>
        set(() => ({ search: input?.trim() || "" })),
      setIsActive: (isActive: PositionsFilterState["isActive"]) =>
        set(() => ({ isActive })),
      setSortBy: (sortBy: PositionsFilterState["sortBy"]) =>
        set(() => ({ sortBy })),
      setDirection: (sortDirection: PositionsFilterState["sortDirection"]) =>
        set(() => ({ sortDirection })),
      setDepartmentIds: (
        departmentIds: PositionsFilterState["departmentIds"]
      ) => set(() => ({ departmentIds })),
    }),
    { name: "position-search", storage: createJSONStorage(() => localStorage) }
  )
);

export const useGetPositionFilter = () => {
  return usePositionsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
      sortBy: state.sortBy,
      sortDirection: state.sortDirection,
      departmentIds: state.departmentIds,
    }))
  );
};

export const setPositionFilterSearch = (
  input: PositionsFilterState["search"]
) => usePositionsFilterStore.getState().setSearch(input);

export const setPositionFilterIsActive = (
  input: PositionsFilterState["isActive"]
) => usePositionsFilterStore.getState().setIsActive(input);

export const setPositionFilterSortBy = (
  input: PositionsFilterState["sortBy"]
) => usePositionsFilterStore.getState().setSortBy(input);

export const setPositionFilterSortDirection = (
  input: PositionsFilterState["sortDirection"]
) => usePositionsFilterStore.getState().setDirection(input);

export const setPositionFilterByDepartments = (
  input: PositionsFilterState["departmentIds"]
) => usePositionsFilterStore.getState().setDepartmentIds(input);
