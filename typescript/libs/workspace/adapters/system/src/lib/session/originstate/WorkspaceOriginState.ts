import { RoleType } from '@allors/workspace/meta/system';
import { IRecord } from '../../IRecord';
import { WorkspaceRecord } from '../../workspace/WorkspaceRecord';
import { Strategy } from '../Strategy';
import { RecordBasedOriginState } from './RecordBasedOriginState';

export class WorkspaceOriginState extends RecordBasedOriginState {

  public constructor(public strategy: Strategy, private workspaceRecord: WorkspaceRecord) {
    super(strategy);
    this.previousRecord = this.workspaceRecord;
  }

  get roleTypes(): Set<RoleType> {
    return this.class.workspaceOriginRoleTypes;
  }

  get record(): IRecord {
    return this.workspaceRecord;
  }

  get Version(): number {
    return this.workspaceRecord.version;
  }

  protected onChange() {
    this.strategy.session.changeSetTracker.onWorkspaceChanged(this);
    this.strategy.session.pushToWorkspaceTracker.onChanged(this);
  }

  public push() {
    if (this.hasChanges) {
      this.workspace.push(this.id, this.class, this.record?.version ?? 0, this.changedRoleByRelationType);
    }

    this.workspaceRecord = this.workspace.getRecord(this.id);
    this.changedRoleByRelationType = null;
  }
}
