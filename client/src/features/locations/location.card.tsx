import StatusBadge from "../status/status.badge";
import { Location } from "@/entities/locations/types";

export default function LocationCard({ location }: { location: Location }) {
  return (
    <div className="flex flex-col justify-between rounded-xl border border-border bg-card p-6 text-card-foreground shadow-sm transition-colors hover:bg-accent/5">
      <div>
        <div className="mb-4 flex items-start justify-between">
          <h3
            className="text-lg font-semibold tracking-tight line-clamp-1"
            title={location.name}
          >
            {location.name}
          </h3>
          <StatusBadge isActive={location.isActive} />
        </div>

        <div className="space-y-1 text-sm text-muted-foreground">
          <p className="flex items-center gap-2">
            <span className="font-medium">Город:</span>
            <span className="text-foreground">{location.city}</span>
          </p>
          <p className="flex items-center gap-2">
            <span className="font-medium">Адрес:</span>
            <span className="text-foreground">
              {location.street}, д. {location.apartamentNumber}
            </span>
          </p>
        </div>
      </div>

      <div className="mt-6 border-t border-border pt-4">
        <button className="w-full cursor-pointer rounded-lg bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground transition-colors hover:bg-secondary/80 active:opacity-90">
          Подробнее
        </button>
      </div>
    </div>
  );
}
