// create a branded iso8601 type to ensure C# can decode this

import { formatISO, parseISO } from 'date-fns';

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
    _value: formatISO(date, { representation: 'date' }),
    _dateValue: date,
  };
}

/** Uses Date.parse to get our branded type */
export function fromDateStringToIso(dateTime: string | Date): ISO8601String {
  const parsedTimestamp =
    typeof dateTime === "string" ? parseISO(dateTime) : dateTime.valueOf();
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
