import { useMemo, useState } from "react";

export type SortDirection = "asc" | "desc";

export interface SortState<K extends string> {
  key: K;
  direction: SortDirection;
}

export interface UseSortableTableResult<T, K extends string> {
  sorted: T[];
  sortState: SortState<K>;
  toggleSort: (key: K) => void;
}

type Comparable = string | number | Date;

function compareValues(a: Comparable, b: Comparable): number {
  if (a instanceof Date && b instanceof Date) {
    return a.getTime() - b.getTime();
  }
  if (typeof a === "string" && typeof b === "string") {
    return a.localeCompare(b);
  }
  if (typeof a === "number" && typeof b === "number") {
    return a - b;
  }
  // Mixed types: fall back to string comparison
  return String(a).localeCompare(String(b));
}

/**
 * Generic client-side sort hook.
 *
 * @param rows        The array to sort (typically already filtered).
 * @param defaultKey  The column key to sort by on first render.
 * @param defaultDir  Initial sort direction (defaults to "asc").
 * @param getValue    Optional accessor: (row, key) => Comparable value.
 *                    Defaults to `row[key]` coerced to Comparable.
 */
export function useSortableTable<
  T extends Record<string, unknown>,
  K extends keyof T & string
>(
  rows: T[] | undefined,
  defaultKey: K,
  defaultDir: SortDirection = "asc",
  getValue?: (row: T, key: K) => Comparable | null | undefined
): UseSortableTableResult<T, K> {
  const [sortState, setSortState] = useState<SortState<K>>({
    key: defaultKey,
    direction: defaultDir,
  });

  const toggleSort = (key: K) => {
    setSortState((prev) =>
      prev.key === key
        ? { key, direction: prev.direction === "asc" ? "desc" : "asc" }
        : { key, direction: "asc" }
    );
  };

  const sorted = useMemo(() => {
    if (!rows) return [];
    const { key, direction } = sortState;
    const multiplier = direction === "asc" ? 1 : -1;

    return [...rows].sort((a, b) => {
      const aVal = getValue ? getValue(a, key) : (a[key] as Comparable | null | undefined);
      const bVal = getValue ? getValue(b, key) : (b[key] as Comparable | null | undefined);

      if (aVal == null && bVal == null) return 0;
      if (aVal == null) return multiplier;
      if (bVal == null) return -multiplier;

      return compareValues(aVal, bVal) * multiplier;
    });
  }, [rows, sortState, getValue]);

  return { sorted, sortState, toggleSort };
}
