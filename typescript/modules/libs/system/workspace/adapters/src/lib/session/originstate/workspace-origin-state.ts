import { IObject } from '@allors/workspace/domain/system';
import { RelationType, RoleType } from '@allors/system/workspace/meta';
import { IRange } from '../../collections/ranges/ranges';
import { IRecord } from '../../irecord';
import { UnknownVersion } from '../../version';
import { WorkspaceRecord } from '../../workspace/workspace-record';
import { WorkspaceResult } from '../../workspace/workspace-result';
import { Session } from '../session';
import { RecordBasedOriginState } from './record-based-origin-state';

export class WorkspaceOriginState extends RecordBasedOriginState {
  protected cachedRoleByRelationType: Map<RelationType, IRange<IObject>>;

  constructor(
    public object: IObject,
    private workspaceRecord: WorkspaceRecord
  ) {
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
    (
      this.object.strategy.session as Session
    ).changeSetTracker.onWorkspaceChanged(this);
    (this.object.strategy.session as Session).pushToWorkspaceTracker.onChanged(
      this
    );
  }

  push() {
    if (this.hasChanges) {
      this.workspace.push(
        this.id,
        this.class,
        this.record?.version ?? UnknownVersion,
        this.changedRoleByRelationType
      );
    }

    this.workspaceRecord = this.workspace.getRecord(this.id);
    this.changedRoleByRelationType = null;
    this.cachedRoleByRelationType = null;
  }

  onPulled(result: WorkspaceResult) {
    const newRecord = this.workspace.getRecord(this.id);
    if (!this.canMerge(newRecord)) {
      result.addMergeError(this.object);
      return;
    }

    this.workspaceRecord = newRecord;
    this.changedRoleByRelationType = null;
    this.cachedRoleByRelationType = null;
  }
}
