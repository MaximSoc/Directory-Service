import { Input } from "@/shared/components/ui/input";
import {
  setDepartmentsFilterSearch,
  useGetDepartmentsFilter,
} from "./model/departments-filter-store";

export function DepartmentsSearch() {
  const { search } = useGetDepartmentsFilter();

  return (
    <div className="flex w-full max-w-md items-center space-x-2">
      <Input
        placeholder="Поиск по названию..."
        value={search}
        onChange={(e) => setDepartmentsFilterSearch(e.target.value)}
      />
    </div>
  );
}
