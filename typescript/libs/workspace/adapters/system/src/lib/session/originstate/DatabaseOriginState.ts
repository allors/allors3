import { Operations } from '@allors/workspace/domain/system';
import { MethodType, RoleType } from '@allors/workspace/meta/system';
import { DatabaseRecord } from '../../database/DatabaseRecord';
import { IRecord } from '../../IRecord';
import { Strategy } from '../Strategy';
import { RecordBasedOriginState } from './RecordBasedOriginState';

export const UnknownVersion = 0;
export const InitialVersion = 1;

export abstract class DatabaseOriginState extends RecordBasedOriginState {
  databaseRecord: DatabaseRecord;

  protected constructor(strategy: Strategy, record: DatabaseRecord) {
    super(strategy);
    this.databaseRecord = record;
    this.previousRecord = this.databaseRecord;
  }

  public get version(): number {
    return this.databaseRecord?.version ?? UnknownVersion;
  }

  private get isVersionUnknown(): boolean {
    return this.version == UnknownVersion;
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

  public canRead(roleType: RoleType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionUnknown) {
      return false;
    }

    const permission = this.session.workspace.database.getPermission(this.class, roleType, Operations.Read);
    return this.databaseRecord.isPermitted(permission);
  }

  public canWrite(roleType: RoleType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionUnknown) {
      return false;
    }

    const permission = this.session.workspace.database.getPermission(this.class, roleType, Operations.Write);
    return this.databaseRecord.isPermitted(permission);
  }

  public canExecute(methodType: MethodType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionUnknown) {
      return false;
    }

    const permission = this.session.workspace.database.getPermission(this.class, methodType, Operations.Execute);
    return this.databaseRecord.isPermitted(permission);
  }

  public pushResponse(newDatabaseRecord: DatabaseRecord) {
    this.databaseRecord = newDatabaseRecord;
    this.changedRoleByRelationType = null;
  }

  public onPulled() {
    this.databaseRecord = this.session.workspace.database.getRecord(this.id);
  }

  onChange() {
    this.session.changeSetTracker.onDatabaseChanged(this);
    this.session.pushToDatabaseTracker.onChanged(this);
  }
}
