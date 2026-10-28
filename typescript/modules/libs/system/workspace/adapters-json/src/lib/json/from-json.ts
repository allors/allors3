import { IUnit } from '@allors/system/workspace/domain';
import { UnitTags } from '@allors/system/workspace/meta';

const integerRange = 'a whole number from -2147483648 through 2147483647';

export function unitFromJson(tag: string, value: unknown): IUnit {
  if (value == null) {
    return null;
  }

  switch (tag) {
    case UnitTags.DateTime:
      if (value instanceof Date) {
        return value;
      }

      if (typeof value === 'string') {
        const date = new Date(value);
        if (!Number.isNaN(date.getTime())) {
          return date;
        }

        throw conversionError(
          tag,
          value,
          `${JSON.stringify(value)} is not a date`
        );
      }

      break;

    case UnitTags.Binary:
      if (typeof value === 'string') {
        return value;
      }
      break;

    case UnitTags.Boolean:
      if (typeof value === 'boolean') {
        return value;
      }
      break;

    case UnitTags.Decimal:
      if (typeof value === 'string') {
        return value;
      }
      break;

    case UnitTags.Float:
      if (typeof value === 'number') {
        return value;
      }
      break;

    case UnitTags.Integer:
      if (typeof value === 'number') {
        if (
          Number.isInteger(value) &&
          value >= -2147483648 &&
          value <= 2147483647
        ) {
          return value;
        }

        throw conversionError(tag, value, `${value} is not ${integerRange}`);
      }
      break;

    case UnitTags.String:
      if (typeof value === 'string') {
        return value;
      }
      break;

    case UnitTags.Unique:
      if (typeof value === 'string') {
        return value;
      }
      break;
  }

  throw conversionError(tag, value);
}

function conversionError(tag: string, value: unknown, reason?: string): Error {
  const unitName =
    Object.entries(UnitTags).find(([, unitTag]) => unitTag === tag)?.[0] ??
    'unknown unit';
  const source = value instanceof Date ? 'Date' : typeof value;
  return new Error(
    `Cannot convert ${source} to ${unitName} (tag '${tag}')${
      reason ? `: ${reason}` : ''
    }.`
  );
}
