import {
  PushRequestNewObject,
  PushRequestObject,
  PushRequestRole,
} from '@allors/system/common/protocol-json';
import {
  DatabaseOriginState as SystemDatabaseOriginState,
  DatabaseRecord,
  Strategy,
  IRange,
} from '@allors/system/workspace/adapters';
import { unitToJson } from '../../json/to-json';
import { IObject } from '@allors/system/workspace/domain';

export class DatabaseOriginState extends SystemDatabaseOriginState {
  constructor(public object: IObject, record: DatabaseRecord) {
    super(record);
  }

  pushNew(): PushRequestNewObject {
    return {
      w: this.id,
      t: this.class.tag,
      r: this.pushRoles(),
    };
  }

  pushExisting(): PushRequestObject {
    return {
      d: this.id,
      v: this.version,
      r: this.pushRoles(),
    };
  }

  private pushRoles(): PushRequestRole[] {
    const ranges = this.session.workspace.ranges;

    if (this.changedRoleByRelationType?.size > 0) {
      const roles: PushRequestRole[] = [];

      for (const [relationType, roleValue] of this.changedRoleByRelationType) {
        const pushRequestRole: PushRequestRole = { t: relationType.tag };

        if (relationType.roleType.objectType.isUnit) {
          pushRequestRole.u = unitToJson(roleValue);
        } else if (relationType.roleType.isOne) {
          pushRequestRole.c = (roleValue as Strategy)?.id;
        } else {
          const roleStrategies = roleValue as IRange<Strategy>;
          const roleIds = ranges.importFrom(roleStrategies?.map((v) => v.id));
          if (!this.existRecord) {
            pushRequestRole.a = ranges.save(roleIds);
          } else {
            const databaseRole = this.databaseRecord.getRole(
              relationType.roleType
            ) as IRange<number>;
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
        }

        roles.push(pushRequestRole);
      }

      return roles;
    }

    return null;
  }
}
