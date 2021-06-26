import { PushRequestNewObject, PushRequestObject, PushRequestRole } from '@allors/protocol/json/system';
import { DatabaseOriginState as SystemDatabaseOriginState, DatabaseRecord, difference, Strategy, toArray } from '@allors/workspace/adapters/system';
import { unitToJson } from '../json/toJson';

export class DatabaseOriginState extends SystemDatabaseOriginState {
  constructor(strategy: Strategy, record: DatabaseRecord) {
    super(strategy, record);
  }

  PushNew(): PushRequestNewObject {
    return {
      w: this.Id,
      t: this.Class.tag,
      r: this.PushRoles(),
    };
  }

  PushExisting(): PushRequestObject {
    return {
      d: this.Id,
      v: this.Version,
      r: this.PushRoles(),
    };
  }

  private PushRoles(): PushRequestRole[] {
    if (this.ChangedRoleByRelationType?.size > 0) {
      const roles: PushRequestRole[] = [];

      for (const [relationType, roleValue] of this.ChangedRoleByRelationType) {
        const pushRequestRole: PushRequestRole = { t: relationType.tag };

        if (relationType.roleType.objectType.isUnit) {
          pushRequestRole.u = unitToJson(roleValue);
        } else if (relationType.roleType.isOne) {
          pushRequestRole.c = roleValue;
        } else if (!this.ExistDatabaseRecord) {
          pushRequestRole.a = toArray(roleValue);
        } else {
          const databaseRole = this.DatabaseRecord.getRole(relationType.roleType);
          if (databaseRole == null) {
            pushRequestRole.a = toArray(roleValue);
          } else {
            pushRequestRole.a = toArray(difference(roleValue, databaseRole));
            pushRequestRole.r = toArray(difference(databaseRole, roleValue));
          }
        }

        roles.push(pushRequestRole);
      }

      return roles;
    }

    return null;
  }
}
