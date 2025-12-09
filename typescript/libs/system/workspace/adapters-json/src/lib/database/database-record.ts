import { Class, RelationType, RoleType } from '@allors/system/workspace/meta';
import {
  DatabaseRecord as SystemDatabaseRecord,
  IRange,
} from '@allors/system/workspace/adapters';
import {
  SyncResponseObject,
  SyncResponseRole,
} from '@allors/system/common/protocol-json';
import { DatabaseConnection } from './database-connection';
import { ResponseContext } from './security/response-context';
import { unitFromJson } from '../json/from-json';

export class DatabaseRecord extends SystemDatabaseRecord {
  grants: IRange<number>;
  revocations: IRange<number>;

  private _roleByRelationType?: Map<RelationType, unknown>;
  private syncResponseRoles?: SyncResponseRole[];

  constructor(
    public readonly database: DatabaseConnection,
    cls: Class,
    id: number,
    public override version: number
  ) {
    super(cls, id, version);
  }

  static fromResponse(
    database: DatabaseConnection,
    ctx: ResponseContext,
    syncResponseObject: SyncResponseObject
  ): DatabaseRecord {
    const object = new DatabaseRecord(
      database,
      database.configuration.metaPopulation.metaObjectByTag.get(
        syncResponseObject.c
      ) as Class,
      syncResponseObject.i,
      syncResponseObject.v
    );
    object.syncResponseRoles = syncResponseObject.ro;
    object.grants = ctx.checkForMissingGrants(syncResponseObject.g);
    object.revocations = ctx.checkForMissingRevocations(syncResponseObject.r);
    return object;
  }

  get roleByRelationType(): Map<RelationType, unknown> {
    if (this.syncResponseRoles != null) {
      const metaPopulation = this.database.configuration.metaPopulation;
      this._roleByRelationType = new Map(
        this.syncResponseRoles.map((v) => {
          const relationType = metaPopulation.metaObjectByTag.get(
            v.t
          ) as RelationType;
          if (relationType == null) {
            throw new Error(
              'RelationType with Tag ' +
                v.t +
                ' is not present. Please regenerate your workspace.'
            );
          }

          const roleType = relationType.roleType;
          const objectType = roleType.objectType;

          let role: unknown;

          if (objectType.isUnit) {
            role = unitFromJson(objectType.tag, v.v);
          } else {
            if (roleType.isOne) {
              role = v.o;
            } else {
              role = v.c;
            }
          }

          return [relationType, role];
        })
      );

      delete this.syncResponseRoles;
    }

    return this._roleByRelationType;
  }

  getRole(roleType: RoleType): unknown {
    return this.roleByRelationType?.get(roleType.relationType);
  }

  isPermitted(permission: number): boolean {
    if (permission == null) {
      return false;
    }

    if (this.grants == null) {
      return false;
    }

    if (
      this.revocations != null &&
      this.revocations.some((v) =>
        this.database.ranges.has(
          this.database.revocationById.get(v).permissionIds,
          permission
        )
      )
    ) {
      return false;
    }

    return this.grants.some((v) =>
      this.database.ranges.has(
        this.database.grantById.get(v).permissionIds,
        permission
      )
    );
  }
}
