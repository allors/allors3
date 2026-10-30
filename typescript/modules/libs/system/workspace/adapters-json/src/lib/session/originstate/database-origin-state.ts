import {
  PushRequestNewObject,
  PushRequestObject,
  PushRequestRole,
} from '@allors/system/common/protocol-json';
import {
  CompositesDelta,
  DatabaseOriginState as SystemDatabaseOriginState,
  DatabaseRecord,
  Strategy,
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
          const delta = roleValue as CompositesDelta;
          pushRequestRole.a = ranges.save(
            ranges.importFrom(delta.added?.map((v) => v.id))
          );
          pushRequestRole.r = ranges.save(delta.removed);
        }

        roles.push(pushRequestRole);
      }

      return roles;
    }

    return null;
  }
}
