import { RoleType } from '@allors/workspace/meta/system';
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

  get RoleTypes(): Set<RoleType> {
    return this.Class.workspaceOriginRoleTypes;
  }

  get Record(): IRecord {
    return this.WorkspaceRecord;
  }

  get Version(): number {
    return this.WorkspaceRecord.version;
  }

  protected /* override */ OnChange() {
    this.Strategy.session.changeSetTracker.OnWorkspaceChanged(this);
    this.Strategy.session.pushToWorkspaceTracker.OnChanged(this);
  }

  public Push() {
    if (this.HasChanges) {
      this.Workspace.push(this.Id, this.Class, this.Record?.version ?? 0, this.ChangedRoleByRelationType);
    }

    this.WorkspaceRecord = this.Workspace.getRecord(this.Id);
    this.ChangedRoleByRelationType = null;
  }
}
