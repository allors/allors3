import { UnitTypes } from '@allors/workspace/domain/system';
import { UnitTags } from '@allors/workspace/meta/system';

export class UnitConvert {
  public ToJson(value: unknown): UnitTypes {
    switch (typeof value) {
      case 'boolean':
      case 'number':
      case 'string':
        return value;
    }

    if (value instanceof Date) {
      return value;
    }

    throw new Error(`Unsupported value: ${value}`);
  }

  public FromJson(tag: number, value: unknown): UnitTypes {
    if (value == null) {
      return null;
    }

    switch (tag) {
      case UnitTags.DateTime:
        if (value instanceof Date) {
          return value;
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
}
