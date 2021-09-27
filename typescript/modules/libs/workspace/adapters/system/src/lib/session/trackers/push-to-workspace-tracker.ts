import { WorkspaceOriginState } from '../originstate/workspace-origin-state';
import { Strategy } from '../strategy';

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
