import {
  PushRequestNewObject,
  PushRequestObject,
  PushRequestRole,
} from '@allors/system/common/protocol-json';
import { ICache } from '../cache/icache';
import { IRange, Ranges } from '../collections/ranges/ranges';
import { PushChangedObject } from '../push/push-changed-object';
import { PushNewObject } from '../push/push-new-object';
import { RoleChange } from '../push/role-change';
import { IRecord } from '../record';
import { unitToJson } from './to-json';

/**
 * Encodes the objects of a push for the wire. A composites role is sent as the ids to add
 * and the ids to remove against the record the cache holds; without a record, as for a new
 * object, every id is an addition. The server compares the version sent with its own, so a
 * record newer than the version sent fails the push before the roles are applied.
 */
export class PushEncoder {
  constructor(
    private readonly cache: ICache,
    private readonly ranges: Ranges<number>
  ) {}

  newObject(newObject: PushNewObject): PushRequestNewObject {
    return {
      w: newObject.workspaceId,
      t: newObject.cls.tag,
      r: this.roles(newObject.roles, undefined),
    };
  }

  changedObject(changedObject: PushChangedObject): PushRequestObject {
    return {
      d: changedObject.id,
      v: changedObject.version,
      r: this.roles(
        changedObject.roles,
        this.cache.getRecord(changedObject.id)
      ),
    };
  }

  private roles(
    roleChanges: RoleChange[] | null | undefined,
    record: IRecord | undefined
  ): PushRequestRole[] {
    if (roleChanges == null || roleChanges.length === 0) {
      return null;
    }

    const ranges = this.ranges;
    const roles: PushRequestRole[] = [];

    for (const { roleType, value } of roleChanges) {
      const pushRequestRole: PushRequestRole = { t: roleType.relationType.tag };

      if (roleType.objectType.isUnit) {
        pushRequestRole.u = unitToJson(value);
      } else if (roleType.isOne) {
        pushRequestRole.c = value as number;
      } else {
        const roleIds = ranges.importFrom(value as number[]);
        const databaseRole =
          record != null
            ? (record.getRole(roleType) as IRange<number>)
            : undefined;

        if (databaseRole == null) {
          pushRequestRole.a = ranges.save(roleIds);
        } else {
          pushRequestRole.a = ranges.save(
            ranges.difference(roleIds, databaseRole)
          );
          pushRequestRole.r = ranges.save(
            ranges.difference(databaseRole, roleIds)
          );
        }
      }

      roles.push(pushRequestRole);
    }

    return roles;
  }
}
