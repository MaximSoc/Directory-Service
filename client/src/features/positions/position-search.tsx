import { Input } from "@/shared/components/ui/input";
import {
  setPositionFilterSearch,
  useGetPositionFilter,
} from "./model/positions-filter-store";

export function PositionsSearch() {
  const { search } = useGetPositionFilter();

  return (
    <div className="flex w-full max-w-md items-center space-x-2">
      <Input
        placeholder="Поиск по названию..."
        value={search}
        onChange={(e) => setPositionFilterSearch(e.target.value)}
      />
    </div>
  );
}
