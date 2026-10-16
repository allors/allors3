import {
  Composite,
  MetaPopulation,
  Origin,
  UnitTags,
} from '@allors/system/workspace/meta';
import { ReadRole } from '@allors/system/common/protocol-json';
import { ReadStoredPermission, ReadStoredRecord } from './read-store-values';

interface RoleMetadata {
  readonly unit?: string;
  readonly many: boolean;
  readonly classes: ReadonlySet<string>;
}

interface ClassMetadata {
  readonly roles: ReadonlyMap<string, RoleMetadata>;
  readonly methods: ReadonlySet<string>;
}

/** Detached workspace-projected metadata, captured before any asynchronous operation. */
export class ReadMetadata {
  #classes = new Map<string, ClassMetadata>();

  constructor(metaPopulation: MetaPopulation) {
    for (const cls of metaPopulation.classes) {
      const roles = new Map<string, RoleMetadata>();
      for (const role of cls.roleTypes) {
        if (role.origin !== Origin.Database) continue;
        roles.set(role.relationType.tag, {
          unit: role.objectType.isUnit ? role.objectType.tag : undefined,
          many: role.isMany,
          classes: new Set(
            role.objectType.isComposite
              ? [...(role.objectType as Composite).classes].map(
                  (target) => target.tag
                )
              : []
          ),
        });
      }
      this.#classes.set(cls.tag, {
        roles,
        methods: new Set([...cls.methodTypes].map((method) => method.tag)),
      });
    }
  }

  requireClass(tag: string): ClassMetadata {
    const cls = this.#classes.get(tag);
    if (!cls)
      throw new Error(
        `Unknown read class ${tag}. Refresh the configured workspace metadata and representation.`
      );
    return cls;
  }

  decodeRoles(
    classTag: string,
    values: readonly ReadRole[]
  ): Record<string, unknown> {
    const cls = this.requireClass(classTag);
    const roles = Object.create(null) as Record<string, unknown>;
    for (const value of values) {
      const role = cls.roles.get(value.t);
      if (!role)
        throw new Error(
          `Role ${value.t} does not belong to read class ${classTag}. Refresh the workspace metadata.`
        );
      if (role.unit) {
        if (value.o != null || value.c != null)
          throw new Error(
            `Invalid unit role ${value.t}. Supply only its encoded unit value.`
          );
        roles[value.t] = decodeUnit(role.unit, value.v ?? null);
      } else if (role.many) {
        if (!Array.isArray(value.c) || value.o != null || value.v != null)
          throw new Error(
            `Invalid to-many role ${value.t}. Supply an explicit array of relation IDs.`
          );
        roles[value.t] = value.c;
      } else {
        if (value.c != null || value.v != null)
          throw new Error(
            `Invalid to-one role ${value.t}. Supply an object ID or null.`
          );
        roles[value.t] = value.o ?? null;
      }
    }
    return roles;
  }

  validateRecord(
    record: ReadStoredRecord,
    pool?: ReadonlyMap<number, ReadStoredRecord>
  ): void {
    const cls = this.requireClass(record.classTag);
    for (const [tag, value] of Object.entries(record.roles)) {
      const role = cls.roles.get(tag);
      if (!role)
        throw new Error(
          `Role ${tag} does not belong to read class ${record.classTag}. Refresh the workspace metadata.`
        );
      if (role.unit) {
        if (role.unit === UnitTags.DateTime && value !== null) {
          if (
            !(value instanceof Date) ||
            !Number.isFinite(value.getTime()) ||
            value.getUTCFullYear() < 1 ||
            value.getUTCFullYear() > 9999
          )
            throw new Error(
              'Invalid date unit. Supply a decoded Date in the supported calendar range.'
            );
        } else decodeUnit(role.unit, value);
      } else {
        const ids = role.many ? value : value === null ? [] : [value];
        if (
          !Array.isArray(ids) ||
          ids.some(
            (id, index) =>
              !Number.isSafeInteger(id) ||
              id <= 0 ||
              (index > 0 && ids[index - 1] >= id)
          )
        )
          throw new Error(
            `Invalid relation role ${tag}. Supply sorted unique positive safe integer IDs with the configured multiplicity.`
          );
        for (const id of ids) {
          const target = pool?.get(id);
          if (target && !role.classes.has(target.classTag))
            throw new Error(
              `Relation role ${tag} references incompatible class ${target.classTag}. Refresh the workspace metadata.`
            );
        }
      }
    }
  }

  validatePermission(permission: ReadStoredPermission): void {
    const cls = this.requireClass(permission.classTag);
    if (permission.operation === 1) return;
    const valid =
      permission.operation === 8
        ? cls.methods.has(permission.operandTag)
        : cls.roles.has(permission.operandTag);
    if (!valid)
      throw new Error(
        `Invalid permission operand ${permission.operandTag} for class ${permission.classTag}. Refresh the workspace metadata and representation.`
      );
  }
}

function decodeUnit(tag: string, value: unknown): unknown {
  if (value === null) return null;
  const text = typeof value === 'string';
  switch (tag) {
    case UnitTags.String:
      if (text) return value;
      break;
    case UnitTags.Boolean:
      if (typeof value === 'boolean') return value;
      break;
    case UnitTags.Integer:
      if (
        typeof value === 'number' &&
        Number.isInteger(value) &&
        value >= -2147483648 &&
        value <= 2147483647
      )
        return value;
      break;
    case UnitTags.Float:
      if (typeof value === 'number' && Number.isFinite(value)) return value;
      break;
    case UnitTags.Decimal:
      if (text && /^-?\d+(?:\.\d+)?$/.test(value)) {
        const parts = value.replace('-', '').split('.');
        const coefficient = parts.join('').replace(/^0+/, '') || '0';
        const maximum = '79228162514264337593543950335';
        if (
          (parts[1]?.length ?? 0) <= 28 &&
          (coefficient.length < maximum.length ||
            (coefficient.length === maximum.length && coefficient <= maximum))
        )
          return value;
      }
      break;
    case UnitTags.Unique:
      if (
        text &&
        /^[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}$/i.test(value)
      )
        return value;
      break;
    case UnitTags.Binary:
      if (
        text &&
        /^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/.test(
          value
        )
      )
        return value;
      break;
    case UnitTags.DateTime:
      if (
        text &&
        /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|[+-]\d{2}:\d{2})$/.test(
          value
        )
      ) {
        const date = new Date(value);
        const month = Number(value.slice(5, 7));
        const day = Number(value.slice(8, 10));
        const year = Number(value.slice(0, 4));
        const offset = /([+-])(\d{2}):(\d{2})$/.exec(value);
        const validOffset =
          !offset ||
          (Number(offset[2]) <= 14 &&
            Number(offset[3]) < 60 &&
            (Number(offset[2]) !== 14 || Number(offset[3]) === 0));
        const days = [
          31,
          year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0) ? 29 : 28,
          31,
          30,
          31,
          30,
          31,
          31,
          30,
          31,
          30,
          31,
        ][month - 1];
        if (
          Number.isFinite(date.getTime()) &&
          validOffset &&
          year >= 1 &&
          date.getUTCFullYear() >= 1 &&
          date.getUTCFullYear() <= 9999 &&
          month >= 1 &&
          month <= 12 &&
          day >= 1 &&
          day <= days &&
          Number(value.slice(11, 13)) < 24 &&
          Number(value.slice(14, 16)) < 60 &&
          Number(value.slice(17, 19)) < 60
        )
          return date;
      }
      break;
  }
  throw new Error(
    `Invalid encoded unit for metadata tag ${tag}. Supply a value in the configured unit representation.`
  );
}
