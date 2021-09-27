import { RoleType } from '@allors/workspace/meta/system';
import { IRecord } from '../../irecord';
import { UnknownVersion } from '../../version';
import { WorkspaceRecord } from '../../workspace/workspace-record';
import { WorkspaceResult } from '../../workspace/workspace-result';
import { Strategy } from '../strategy';
import { RecordBasedOriginState } from './record-based-origin-state';

export class WorkspaceOriginState extends RecordBasedOriginState {
  constructor(public strategy: Strategy, private workspaceRecord: WorkspaceRecord) {
    super();
    this.previousRecord = this.workspaceRecord;
  }

  get roleTypes(): Set<RoleType> {
    return this.class.workspaceOriginRoleTypes;
  }

  get record(): IRecord {
    return this.workspaceRecord;
  }

  get version(): number {
    return this.workspaceRecord.version;
  }

  protected onChange() {
    this.strategy.session.changeSetTracker.onWorkspaceChanged(this);
    this.strategy.session.pushToWorkspaceTracker.onChanged(this);
  }

  push() {
    if (this.hasChangedRoles) {
      this.workspace.push(this.id, this.class, this.record?.version ?? UnknownVersion, this.changedRoleByRelationType);
    }

    this.workspaceRecord = this.workspace.getRecord(this.id);
    this.changedRoleByRelationType = null;
  }

  onPulled(result: WorkspaceResult) {
    const newRecord = this.workspace.getRecord(this.id);
    if (!this.canMerge(newRecord)) {
      result.addMergeError(this.strategy.object);
      return;
    }

    this.workspaceRecord = newRecord;
    this.changedRoleByRelationType = null;
  }
}
