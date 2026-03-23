// create a branded iso8601 type to ensure C# can decode this

const DateTimeSym: unique symbol = Symbol.for("utils/datetime/iso8601");
type DateTimeSym = typeof DateTimeSym;

/** Branded type to enforce ISO8601 formatting */
export type ISO8601String = {
  readonly [DateTimeSym]: "utils/datetime/iso8601";
  readonly _dateValue: Date;
  readonly _value: string;
};

/** Format as YYYY-MM-DD to avoid timezone shifting */
function toLocalDateString(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, "0");
  const d = String(date.getDate()).padStart(2, "0");
  return `${y}-${m}-${d}T00:00:00`;
}

/** Any date will pass through transparently */
function fromDate(date: Date): ISO8601String {
  return {
    [DateTimeSym]: "utils/datetime/iso8601",
    _value: toLocalDateString(date),
    _dateValue: date,
  };
}

/** Uses Date.parse to get our branded type */
export function fromDateStringToIso(dateTime: string | Date): ISO8601String {
  const parsedTimestamp =
    typeof dateTime === "string" ? Date.parse(dateTime) : dateTime.valueOf();
  if (Number.isNaN(parsedTimestamp)) {
    throw new Error("Invalid date time string.");
  }

  return fromDate(new Date(parsedTimestamp));
}

export function getNowIso(): ISO8601String {
  return fromDate(new Date());
}

export function toIsoStr(branded: ISO8601String): string {
  return branded._value;
}
