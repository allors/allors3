import { RoleType } from '@allors/workspace/domain/system';
import { IRecord } from '../../IRecord';
import { WorkspaceRecord } from '../../workspace/WorkspaceRecord';
import { Strategy } from '../Strategy';
import { RecordBasedOriginState } from './RecordBasedOriginState';

export /* sealed */ class WorkspaceOriginState extends RecordBasedOriginState {
  public Strategy: Strategy;
  private WorkspaceRecord: WorkspaceRecord;

  public constructor(strategy: Strategy, record: WorkspaceRecord) {
    super();
    this.Strategy = strategy;
    this.WorkspaceRecord = record;
    this.PreviousRecord = this.WorkspaceRecord;
  }

  get RoleTypes(): RoleType[] {
    return this.Class.WorkspaceOriginRoleTypes;
  }

  get Record(): IRecord {
    return this.WorkspaceRecord;
  }

  get Version(): number {
    return this.WorkspaceRecord.version;
  }

  protected /* override */ OnChange() {
    this.Strategy.Session.ChangeSetTracker.OnChanged(this);
    this.Strategy.Session.PushToWorkspaceTracker.OnChanged(this);
  }

  public Push() {
    if (this.HasChanges) {
      this.Workspace.push(this.Id, this.Class, this.Record?.version ?? 0, this.ChangedRoleByRelationType);
    }

    this.WorkspaceRecord = this.Workspace.GetRecord(this.Id);
    this.ChangedRoleByRelationType = null;
  }
}
