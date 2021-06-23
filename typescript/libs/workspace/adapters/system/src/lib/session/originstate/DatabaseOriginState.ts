import { Operations } from '@allors/workspace/domain/system';
import { MethodType, RoleType } from '@allors/workspace/domain/system';
import { DatabaseRecord } from '../../database/DatabaseRecord';
import { IRecord } from '../../IRecord';
import { RecordBasedOriginState } from './RecordBasedOriginState';

export const UnknownVersion = 0;
export const InitialVersion = 1;

export class DatabaseOriginState extends RecordBasedOriginState {
  DatabaseRecord: DatabaseRecord;

  protected constructor(record: DatabaseRecord) {
    super();
    this.DatabaseRecord = record;
    this.PreviousRecord = this.DatabaseRecord;
  }

  public get Version(): number {
    return this.DatabaseRecord?.version ?? UnknownVersion;
  }

  private get IsVersionUnknown(): boolean {
    return this.Version == UnknownVersion;
  }

  protected get ExistDatabaseRecord(): boolean {
    return this.Record != null;
  }

  get RoleTypes(): RoleType[] {
    return this.Class.databaseOriginRoleTypes;
  }

  get Record(): IRecord {
    return this.DatabaseRecord;
  }

  public CanRead(roleType: RoleType): boolean {
    if (!this.ExistDatabaseRecord) {
      return true;
    }

    if (this.IsVersionUnknown) {
      return false;
    }

    const permission = this.Session.workspace.DatabaseConnection.GetPermission(this.Class, roleType, Operations.Read);
    return this.DatabaseRecord.isPermitted(permission);
  }

  public CanWrite(roleType: RoleType): boolean {
    if (!this.ExistDatabaseRecord) {
      return true;
    }

    if (this.IsVersionUnknown) {
      return false;
    }

    const permission = this.Session.workspace.DatabaseConnection.GetPermission(this.Class, roleType, Operations.Write);
    return this.DatabaseRecord.isPermitted(permission);
  }

  public CanExecute(methodType: MethodType): boolean {
    if (!this.ExistDatabaseRecord) {
      return true;
    }

    if (this.IsVersionUnknown) {
      return false;
    }

    const permission = this.Session.workspace.DatabaseConnection.GetPermission(this.Class, methodType, Operations.Execute);
    return this.DatabaseRecord.isPermitted(permission);
  }

  public PushResponse(newDatabaseRecord: DatabaseRecord) {
    this.DatabaseRecord = newDatabaseRecord;
    this.ChangedRoleByRelationType = null;
  }

  public OnPulled() {}

  protected /* override */ OnChange() {
    this.Session.changeSetTracker.OnChanged(this);
    this.Session.pushToDatabaseTracker.OnChanged(this);
  }
}
