import {
  IObject,
  IPullResult,
  Operations,
} from '@allors/system/workspace/domain';
import {
  MethodType,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';
import { DatabaseRecord } from '../../database/database-record';
import { WorkspaceInitialVersion } from '../../version';
import { IRecord } from '../../irecord';
import { RecordBasedOriginState } from './record-based-origin-state';
import { IRange } from '../../collections/ranges/ranges';

export abstract class DatabaseOriginState extends RecordBasedOriginState {
  protected cachedRoleByRelationType: Map<RelationType, IRange<IObject>>;

  protected isPushed: boolean;

  protected constructor(public databaseRecord: DatabaseRecord) {
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

  canRead(roleType: RoleType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionInitial) {
      // TODO: Security
      return true;
    }

    const permission = this.session.workspace.database.getPermission(
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

    const permission = this.session.workspace.database.getPermission(
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

    const permission = this.session.workspace.database.getPermission(
      this.class,
      methodType,
      Operations.Execute
    );
    return this.databaseRecord.isPermitted(permission);
  }

  onPushed() {
    this.isPushed = true;
  }

  canAcceptRecord(record: DatabaseRecord): boolean {
    return this.isPushed || this.canMerge(record);
  }

  /** Apply only after the owner has staged merge checks and admitted the generation. */
  acceptRecord(record: DatabaseRecord): void {
    if (this.isPushed) {
      this.changedRoleByRelationType = null;
      this.isPushed = false;
    }
    this.databaseRecord = record;
    this.cachedRoleByRelationType = null;
  }

  onPulled(pull: IPullResult, exactRecord?: DatabaseRecord) {
    const newRecord =
      exactRecord ?? this.session.workspace.database.getRecord(this.id);
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
}
