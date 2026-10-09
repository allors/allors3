import {
  Class,
  MetaPopulation,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';
import { SyncResponseObject } from '@allors/system/common/protocol-json';
import { Grant } from './grant';
import { loadRange } from './collections/ranges/load-range';
import { IRange, Ranges } from './collections/ranges/ranges';
import { unitFromJson } from './json/from-json';
import { ResponseContext } from './response-context';
import { Revocation } from './revocation';

/**
 * A database object as the connection received it: one user's view of the object at the
 * version it had, immutable. A role is a value: a unit, the id of a composite role, or the
 * sorted ids of a composites role as an IRange of number. A role the user may not read is
 * absent.
 */
export interface IRecord {
  readonly cls: Class;

  readonly id: number;

  readonly version: number;

  /**
   * The ids of the grants that apply to the user on this object.
   */
  readonly grantIds: IRange<number>;

  /**
   * The ids of the revocations that apply to the user on this object.
   */
  readonly revocationIds: IRange<number>;

  getRole(roleType: RoleType): unknown;

  /**
   * Whether the user holds the permission on this object: granted by one of the object's
   * grants and denied by none of its revocations, as the connection holds them now.
   */
  isPermitted(permission: number): boolean;
}

/**
 * A record as a sync response delivered it, converted from the wire when it is built and
 * immutable after that. The permissions are answered against the grants and revocations the
 * connection holds at the time of asking.
 */
export class Record implements IRecord {
  readonly cls: Class;

  readonly id: number;

  readonly version: number;

  readonly grantIds: IRange<number>;

  readonly revocationIds: IRange<number>;

  private readonly roleByRelationType: Map<RelationType, unknown>;

  constructor(
    private readonly grantById: ReadonlyMap<number, Grant>,
    private readonly revocationById: ReadonlyMap<number, Revocation>,
    metaPopulation: MetaPopulation,
    private readonly ranges: Ranges<number>,
    ctx: ResponseContext,
    syncResponseObject: SyncResponseObject
  ) {
    this.cls = metaPopulation.metaObjectByTag.get(
      syncResponseObject.c
    ) as Class;
    if (this.cls == null) {
      throw new Error(
        `Class with tag ${syncResponseObject.c} is not present. Please regenerate your workspace.`
      );
    }

    this.id = syncResponseObject.i;
    this.version = syncResponseObject.v;
    this.grantIds = loadRange(
      ranges,
      ctx.checkForMissingGrants(syncResponseObject.g)
    );
    this.revocationIds = loadRange(
      ranges,
      ctx.checkForMissingRevocations(syncResponseObject.r)
    );

    this.roleByRelationType = new Map(
      (syncResponseObject.ro ?? []).map((v) => {
        const relationType = metaPopulation.metaObjectByTag.get(
          v.t
        ) as RelationType;
        if (relationType == null) {
          throw new Error(
            `RelationType with tag ${v.t} is not present. Please regenerate your workspace.`
          );
        }

        const roleType = relationType.roleType;
        const objectType = roleType.objectType;

        let role: unknown;
        if (objectType.isUnit) {
          role = unitFromJson(objectType.tag, v.v);
        } else if (roleType.isOne) {
          role = v.o;
        } else {
          role = v.c;
        }

        return [relationType, role];
      })
    );
  }

  getRole(roleType: RoleType): unknown {
    return this.roleByRelationType.get(roleType.relationType);
  }

  isPermitted(permission: number): boolean {
    if (permission == null) {
      return false;
    }

    // A grant or revocation the connection no longer holds grants nothing and denies nothing.
    if (
      this.revocationIds != null &&
      this.revocationIds.some((v) => {
        const revocation = this.revocationById.get(v);
        return (
          revocation != null &&
          this.ranges.has(revocation.permissionIds, permission)
        );
      })
    ) {
      return false;
    }

    return (
      this.grantIds != null &&
      this.grantIds.some((v) => {
        const grant = this.grantById.get(v);
        return (
          grant != null && this.ranges.has(grant.permissionIds, permission)
        );
      })
    );
  }
}
