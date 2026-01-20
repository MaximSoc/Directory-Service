export default function StatusBadge({ isActive }: { isActive: boolean }) {
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 ${
        isActive
          ? "border-transparent bg-green-500/15 text-green-700 dark:text-green-400"
          : "border-transparent bg-destructive/15 text-destructive"
      }`}
    >
      {isActive ? "Активен" : "Неактивен"}
    </span>
  );
}
