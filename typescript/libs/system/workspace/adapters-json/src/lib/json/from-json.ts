import { IUnit } from '@allors/system/workspace/domain';
import { UnitTags } from '@allors/system/workspace/meta';

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
        return new Date(value);
      }

      break;

    case UnitTags.Binary:
      if (typeof value === 'string') {
        return btoa(value);
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
        return value;
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

  throw new Error(`Unsupported value: ${value} for tab ${tag}`);
}
