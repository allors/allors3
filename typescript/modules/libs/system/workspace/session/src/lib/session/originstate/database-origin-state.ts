import {
  IDatabaseConnection,
  IRange,
  IRecord,
  Operations,
  PushChangedObject,
  PushNewObject,
  RoleChange,
  WorkspaceInitialVersion,
} from '@allors/system/workspace/connection';
import { IObject, IPullResult } from '@allors/system/workspace/domain';
import {
  MethodType,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';
import { RecordBasedOriginState } from './record-based-origin-state';

export class DatabaseOriginState extends RecordBasedOriginState {
  protected cachedRoleByRelationType: Map<RelationType, IRange<IObject>>;

  private isPushed: boolean;

  constructor(public object: IObject, public databaseRecord: IRecord | undefined) {
    super();
    this.previousRecord = this.databaseRecord;
    this.isPushed = false;
  }

  get version(): number {
    return this.databaseRecord?.version ?? WorkspaceInitialVersion;
  }

  private get isVersionInitial(): boolean {
    return this.version == WorkspaceInitialVersion;
  }

  get roleTypes(): Set<RoleType> {
    return this.class.databaseOriginRoleTypes;
  }

  protected get existRecord(): boolean {
    return this.record != null;
  }

  get record(): IRecord {
    return this.databaseRecord;
  }

  private get connection(): IDatabaseConnection {
    return this.session.workspace.connection;
  }

  canRead(roleType: RoleType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionInitial) {
      // TODO: Security
      return true;
    }

    const permission = this.connection.getPermission(
      this.class,
      roleType,
      Operations.Read
    );
    return this.databaseRecord.isPermitted(permission);
  }

  canWrite(roleType: RoleType): boolean {
    if (this.isVersionInitial) {
      return !this.isPushed;
    }

    if (this.isPushed) {
      return false;
    }

    if (!this.existRecord) {
      return true;
    }

    const permission = this.connection.getPermission(
      this.class,
      roleType,
      Operations.Write
    );
    return this.databaseRecord.isPermitted(permission);
  }

  canExecute(methodType: MethodType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionInitial) {
      // TODO: Security
      return false;
    }

    const permission = this.connection.getPermission(
      this.class,
      methodType,
      Operations.Execute
    );
    return this.databaseRecord.isPermitted(permission);
  }

  onPushed() {
    this.isPushed = true;
  }

  onPulled(pull: IPullResult) {
    const newRecord = this.connection.getRecord(this.id);
    if (!this.isPushed) {
      if (!this.canMerge(newRecord)) {
        pull.addMergeError(this.object);
        return;
      }
    } else {
      this.changedRoleByRelationType = null;
      this.isPushed = false;
    }

    this.databaseRecord = newRecord;
    this.cachedRoleByRelationType = null;
  }

  onChange() {
    this.session.changeSetTracker.onDatabaseChanged(this);
    this.session.pushToDatabaseTracker.onChanged(this);
  }

  pushNew(): PushNewObject {
    return {
      workspaceId: this.id,
      cls: this.class,
      roles: this.roleChanges(),
    };
  }

  pushExisting(): PushChangedObject {
    return {
      id: this.id,
      version: this.version,
      roles: this.roleChanges(),
    };
  }

  /**
   * The changed roles in the shape the connection takes: a unit, the id of a composite
   * role, or the ids of a composites role.
   */
  private roleChanges(): RoleChange[] | null {
    if (!(this.changedRoleByRelationType?.size > 0)) {
      return null;
    }

    const roleChanges: RoleChange[] = [];

    for (const [relationType, roleValue] of this.changedRoleByRelationType) {
      const roleType = relationType.roleType;

      let value: RoleChange['value'];
      if (roleType.objectType.isUnit) {
        value = roleValue as RoleChange['value'];
      } else if (roleType.isOne) {
        value = (roleValue as IObject)?.id;
      } else {
        value = (roleValue as IRange<IObject>)?.map((v) => v.id);
      }

      roleChanges.push({ roleType, value });
    }

    return roleChanges;
  }
}
