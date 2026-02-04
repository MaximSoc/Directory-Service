import { create } from "zustand";
import { PAGE_SIZE } from "./use-locations-list";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { SortDirection } from "@/shared/types/custom-types";

export type LocationSortField =
  | "name"
  | "city"
  | "country"
  | "region"
  | "isActive";

export type LocationsFilterState = {
  search: string;
  isActive?: boolean;
  pageSize: number;
  sortBy: LocationSortField;
  sortDirection: SortDirection;
};

type Actions = {
  setSearch: (input: LocationsFilterState["search"]) => void;
  setIsActive: (isActive: LocationsFilterState["isActive"]) => void;
  setSortBy: (sortBy: LocationsFilterState["sortBy"]) => void;
  setDirection: (sortDirection: LocationsFilterState["sortDirection"]) => void;
};

type LocationsFilterStore = LocationsFilterState & Actions;

const initialState: LocationsFilterState = {
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE,
  sortBy: "name",
  sortDirection: "asc",
};

export const useLocationsFilterStore = create<LocationsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setSearch: (input: LocationsFilterState["search"]) =>
        set(() => ({ search: input?.trim() || "" })),
      setIsActive: (isActive: LocationsFilterState["isActive"]) =>
        set(() => ({ isActive })),
      setSortBy: (sortBy: LocationsFilterState["sortBy"]) =>
        set(() => ({ sortBy })),
      setDirection: (sortDirection: LocationsFilterState["sortDirection"]) =>
        set(() => ({ sortDirection })),
    }),
    { name: "location-search", storage: createJSONStorage(() => localStorage) }
  )
);

export const useGetLocationFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
      sortBy: state.sortBy,
      sortDirection: state.sortDirection,
    }))
  );
};

export const setLocationFilterSearch = (
  input: LocationsFilterState["search"]
) => useLocationsFilterStore.getState().setSearch(input);

export const setLocationFilterIsActive = (
  input: LocationsFilterState["isActive"]
) => useLocationsFilterStore.getState().setIsActive(input);

export const setLocationFilterSortBy = (
  input: LocationsFilterState["sortBy"]
) => useLocationsFilterStore.getState().setSortBy(input);

export const setLocationFilterSortDirection = (
  input: LocationsFilterState["sortDirection"]
) => useLocationsFilterStore.getState().setDirection(input);
