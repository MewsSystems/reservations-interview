// create a branded iso8601 type to ensure C# can decode this

const DateTimeSym: unique symbol = Symbol.for("utils/datetime/iso8601");
type DateTimeSym = typeof DateTimeSym;

/** Branded type to enforce ISO8601 formatting */
export type ISO8601String = {
  readonly [DateTimeSym]: "utils/datetime/iso8601";
  readonly _dateValue: Date;
  readonly _value: string;
};

/** Any date will pass through transparently */
function fromDate(date: Date): ISO8601String {
  return {
    [DateTimeSym]: "utils/datetime/iso8601",
    _value: date.toISOString(),
    _dateValue: date,
  };
}

/** Uses Date.parse to get our branded type */
export function fromDateStringToIso(dateTimeString: string): ISO8601String {
  const parsedTimestamp = Date.parse(dateTimeString);
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
