import { PushRequestNewObject, PushRequestObject, PushRequestRole } from '@allors/protocol/json/system';
import { DatabaseOriginState as SystemDatabaseOriginState, DatabaseRecord, difference, Strategy, save, IRange, importFrom } from '@allors/workspace/adapters/system';
import { unitToJson } from '../../json/toJson';

export class DatabaseOriginState extends SystemDatabaseOriginState {
  constructor(public strategy: Strategy, record: DatabaseRecord) {
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
    if (this.xchangedRoleByRelationType?.size > 0) {
      const roles: PushRequestRole[] = [];

      for (const [relationType, roleValue] of this.xchangedRoleByRelationType) {
        const pushRequestRole: PushRequestRole = { t: relationType.tag };

        if (relationType.roleType.objectType.isUnit) {
          pushRequestRole.u = unitToJson(roleValue);
        } else if (relationType.roleType.isOne) {
          pushRequestRole.c = (roleValue as Strategy)?.id;
        } else {
          const roleIds = importFrom([...(roleValue as Set<Strategy>)].map((v) => v.id));
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
