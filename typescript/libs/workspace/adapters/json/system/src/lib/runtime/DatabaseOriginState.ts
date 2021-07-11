import { PushRequestNewObject, PushRequestObject, PushRequestRole } from '@allors/protocol/json/system';
import { DatabaseOriginState as SystemDatabaseOriginState, DatabaseRecord, difference, Strategy, save, IRange } from '@allors/workspace/adapters/system';
import { unitToJson } from '../json/toJson';

export class DatabaseOriginState extends SystemDatabaseOriginState {
  constructor(strategy: Strategy, record: DatabaseRecord) {
    super(strategy, record);
  }

  PushNew(): PushRequestNewObject {
    return {
      w: this.id,
      t: this.class.tag,
      r: this.PushRoles(),
    };
  }

  PushExisting(): PushRequestObject {
    return {
      d: this.id,
      v: this.version,
      r: this.PushRoles(),
    };
  }

  private PushRoles(): PushRequestRole[] {
    if (this.changedRoleByRelationType?.size > 0) {
      const roles: PushRequestRole[] = [];

      for (const [relationType, roleValue] of this.changedRoleByRelationType) {
        const pushRequestRole: PushRequestRole = { t: relationType.tag };

        if (relationType.roleType.objectType.isUnit) {
          pushRequestRole.u = unitToJson(roleValue);
        } else if (relationType.roleType.isOne) {
          pushRequestRole.c = roleValue as number;
        } else {
          const roleIds = roleValue as IRange;
          if (!this.existRecord) {
            pushRequestRole.a = save(roleIds);
          } else {
            const databaseRole = this.databaseRecord.getRole(relationType.roleType) as IRange;
            if (databaseRole == null) {
              pushRequestRole.a = save(roleIds);
            } else {
              pushRequestRole.a = save(difference(roleIds, databaseRole));
              pushRequestRole.r = save(difference(databaseRole, roleIds));
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
