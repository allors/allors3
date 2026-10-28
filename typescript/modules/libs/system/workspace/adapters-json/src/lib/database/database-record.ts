import {
  Class,
  RelationType,
  RoleType,
  UnitTags,
} from '@allors/system/workspace/meta';
import {
  DatabaseRecord as SystemDatabaseRecord,
  DefaultNumberRanges,
} from '@allors/system/workspace/adapters';
import { SyncResponseObject } from '@allors/system/common/protocol-json';
import { DatabaseConnection } from './database-connection';
import { AccessEntry, ResponseContext } from './security/response-context';
import { unitFromJson } from '../json/from-json';

export class DatabaseRecord extends SystemDatabaseRecord {
  private static readonly ranges = new DefaultNumberRanges();
  readonly grants: readonly number[];
  readonly revocations: readonly number[];
  readonly key: string;

  private constructor(
    cls: Class,
    id: number,
    version: number,
    private readonly grantEntries: readonly AccessEntry[],
    private readonly revocationEntries: readonly AccessEntry[],
    private readonly roleByRelationType: Map<RelationType, unknown>
  ) {
    super(cls, id, version);
    this.grants = Object.freeze(grantEntries.map((v) => v.id));
    this.revocations = Object.freeze(revocationEntries.map((v) => v.id));
    this.key = ResponseContext.payloadKey(
      id,
      version,
      grantEntries.map((v) => [v.id, v.version]),
      revocationEntries.map((v) => [v.id, v.version])
    );
    Object.freeze(this);
  }

  static fromResponse(
    database: DatabaseConnection,
    wire: SyncResponseObject,
    grants: AccessEntry[],
    revocations: AccessEntry[]
  ): DatabaseRecord {
    const meta = database.configuration.metaPopulation.metaObjectByTag;
    const cls = meta.get(wire.c) as Class;
    if (cls?.kind !== 'Class') {
      throw new Error('Sync object references an unknown class.');
    }
    const roles = new Map<RelationType, unknown>();
    for (const role of ResponseContext.array(wire.ro, 'Sync roles')) {
      ResponseContext.entry(role, 'Role');
      const relation = meta.get(role.t) as RelationType;
      const roleType = relation?.roleType;
      if (!roleType) {
        throw new Error(
          `Sync object ${wire.i} (class '${wire.c}') references unknown role tag '${role.t}'. Check that the client and server use matching workspace models; regenerate your workspace if needed.`
        );
      }
      if (!cls.databaseOriginRoleTypes.has(roleType)) {
        throw new Error(
          `Sync object ${wire.i} (class '${wire.c}') contains role tag '${role.t}' that is not a database role of this class.`
        );
      }
      if (roles.has(relation)) {
        throw new Error(
          `Sync object ${wire.i} (class '${wire.c}') contains duplicate role tag '${role.t}'. Return each role only once.`
        );
      }
      let value: unknown;
      if (roleType.objectType.isUnit) {
        if (role.o != null || role.c != null) {
          throw new Error(
            'Sync unit roles cannot contain relationship members.'
          );
        }
        if (
          roleType.objectType.tag === UnitTags.DateTime &&
          typeof role.v === 'string'
        ) {
          DatabaseRecord.validateDate(role.v);
        }
        value = unitFromJson(roleType.objectType.tag, role.v);
        if (typeof value === 'string') {
          DatabaseRecord.validateEncoding(roleType.objectType.tag, value);
        }
        if (
          value != null &&
          roleType.objectType.tag === UnitTags.Integer &&
          (!Number.isInteger(value) ||
            (value as number) < -2147483648 ||
            (value as number) > 2147483647)
        ) {
          throw new Error('Sync integer roles must contain a 32-bit integer.');
        }
        if (
          value != null &&
          roleType.objectType.tag === UnitTags.Float &&
          !Number.isFinite(value)
        ) {
          throw new Error('Sync float roles must contain a finite number.');
        }
        if (value instanceof Date) {
          if (!Number.isFinite(value.getTime())) {
            throw new Error('Sync contains an invalid date.');
          }
          value = new Date(value.getTime());
        }
      } else if (roleType.isOne) {
        if (role.v != null || role.c != null) {
          throw new Error(
            'Sync to-one roles cannot contain unit or collection members.'
          );
        }
        value = role.o == null || role.o === 0 ? undefined : role.o;
        if (value != null) {
          ResponseContext.id(value as number);
        }
      } else {
        if (role.v != null || role.o != null) {
          throw new Error(
            'Sync to-many roles cannot contain unit or object members.'
          );
        }
        value = Object.freeze(ResponseContext.ids(role.c));
      }
      roles.set(relation, value);
    }
    return new DatabaseRecord(
      cls,
      wire.i,
      wire.v,
      Object.freeze([...grants]),
      Object.freeze([...revocations]),
      roles
    );
  }

  private static validateDate(value: string): void {
    const match =
      /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})(?:\.\d{1,7})?(?:Z|([+-])(\d{2}):(\d{2}))?$/.exec(
        value
      );
    if (!match) {
      throw new Error('Sync date roles must contain an ISO date and time.');
    }
    const [
      ,
      yearText,
      monthText,
      dayText,
      hourText,
      minuteText,
      secondText,
      ,
      offsetHourText,
      offsetMinuteText,
    ] = match;
    const [year, month, day, hour, minute, second] = [
      yearText,
      monthText,
      dayText,
      hourText,
      minuteText,
      secondText,
    ].map(Number);
    const leap = year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
    const days = [31, leap ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    const offsetHour = Number(offsetHourText ?? 0);
    const offsetMinute = Number(offsetMinuteText ?? 0);
    if (
      year < 1 ||
      month < 1 ||
      month > 12 ||
      day < 1 ||
      day > days[month - 1] ||
      hour > 23 ||
      minute > 59 ||
      second > 59 ||
      offsetHour > 14 ||
      offsetMinute > 59 ||
      (offsetHour === 14 && offsetMinute !== 0)
    ) {
      throw new Error(
        'Sync date roles must contain a valid calendar date, time and UTC offset.'
      );
    }
  }

  private static validateEncoding(tag: string, value: string): void {
    if (
      tag === UnitTags.Binary &&
      !/^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/.test(
        value
      )
    ) {
      throw new Error('Sync binary roles must contain valid Base64.');
    }
    if (
      tag === UnitTags.Unique &&
      !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
        value
      )
    ) {
      throw new Error(
        'Sync unique roles must contain a GUID in hyphenated form.'
      );
    }
    if (tag === UnitTags.Decimal) {
      const decimal = value.trim();
      const [integer, fraction = ''] = decimal.replace(/^[+-]/, '').split('.');
      const significantFraction = fraction.replace(/0+$/, '');
      const coefficient = (integer + significantFraction).replace(/^0+/, '');
      const maximum = '79228162514264337593543950335';
      if (
        !/^[+-]?(?:\d+(?:\.\d*)?|\.\d+)$/.test(decimal) ||
        significantFraction.length > 28 ||
        coefficient.length > maximum.length ||
        (coefficient.length === maximum.length && coefficient > maximum)
      ) {
        throw new Error(
          'Sync decimal roles must contain a decimal value within the supported range.'
        );
      }
    }
  }

  getRole(roleType: RoleType): unknown {
    const value = this.roleByRelationType.get(roleType.relationType);
    return value instanceof Date ? new Date(value.getTime()) : value;
  }

  samePayload(other: DatabaseRecord): boolean {
    if (
      this.cls !== other.cls ||
      this.roleByRelationType.size !== other.roleByRelationType.size
    ) {
      return false;
    }
    for (const [relation, value] of this.roleByRelationType) {
      if (!other.roleByRelationType.has(relation)) {
        return false;
      }
      const otherValue = other.roleByRelationType.get(relation);
      if (value instanceof Date && otherValue instanceof Date) {
        if (value.getTime() !== otherValue.getTime()) {
          return false;
        }
      } else if (Array.isArray(value) && Array.isArray(otherValue)) {
        if (!ResponseContext.sameIds(value, otherValue)) {
          return false;
        }
      } else if (value !== otherValue) {
        return false;
      }
    }
    return true;
  }

  isPermitted(permission: number): boolean {
    return (
      permission != null &&
      !this.revocationEntries.some((entry) =>
        DatabaseRecord.ranges.has(entry.permissionIds as number[], permission)
      ) &&
      this.grantEntries.some((entry) =>
        DatabaseRecord.ranges.has(entry.permissionIds as number[], permission)
      )
    );
  }
}
