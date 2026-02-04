import { LocationsSearch } from "./locations-search";
import { LocationStatusFilter } from "./locations-status-filter";
import { LocationSortControls } from "./location-sort-controls";

export function LocationsFilters() {
  return (
    <div className="flex w-full flex-col xl:flex-row items-start xl:items-center gap-4">
      <LocationsSearch />

      <div className="flex flex-wrap items-center gap-2">
        <LocationStatusFilter />

        <div className="flex items-center space-x-2 border-l pl-2 ml-2 border-border/50">
          <LocationSortControls />
        </div>
      </div>
    </div>
  );
}
