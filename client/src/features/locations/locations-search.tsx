import {
  setLocationFilterSearch,
  useGetLocationFilter,
} from "./model/locations-filter-store";
import { Input } from "@/shared/components/ui/input";

export function LocationsSearch() {
  const { search } = useGetLocationFilter();

  return (
    <div className="flex w-full max-w-md items-center space-x-2">
      <Input
        placeholder="Поиск по названию..."
        value={search}
        onChange={(e) => setLocationFilterSearch(e.target.value)}
      />
    </div>
  );
}
