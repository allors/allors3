import { WorkspaceOriginState } from '../originstate/WorkspaceOriginState';
import { Strategy } from '../Strategy';

export class PushToWorkspaceTracker {
  created: Set<Strategy>;
  changed: Set<WorkspaceOriginState>;

  public onCreated(strategy: Strategy) {
    (this.created ??= new Set<Strategy>()).add(strategy);
  }

  public onChanged(state: WorkspaceOriginState) {
    (this.changed ??= new Set<WorkspaceOriginState>()).add(state);
  }
}
