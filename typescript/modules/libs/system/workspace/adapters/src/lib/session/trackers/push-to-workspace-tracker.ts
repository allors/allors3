import { IObject } from '@allors/workspace/domain/system';
import { WorkspaceOriginState } from '../originstate/workspace-origin-state';

export class PushToWorkspaceTracker {
  created: Set<IObject>;
  changed: Set<WorkspaceOriginState>;

  public onCreated(strategy: IObject) {
    (this.created ??= new Set<IObject>()).add(strategy);
  }

  public onChanged(state: WorkspaceOriginState) {
    (this.changed ??= new Set<WorkspaceOriginState>()).add(state);
  }

  onDelete(object: IObject) {
    this.created?.delete(object);
  }
}
