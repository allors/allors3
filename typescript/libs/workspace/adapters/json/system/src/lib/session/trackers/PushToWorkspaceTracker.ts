import { AssociationType, RoleType } from '@allors/workspace/meta/system';
import { Strategy } from '../Strategy';

export /* sealed */ class PushToWorkspaceTracker {

  Created: Set<Strategy>;
  Changed: Set<WorkspaceOriginState>;

  public OnCreated(strategy: Strategy) {
    (this.Created ??= new Set<Strategy>()).add(strategy);
  }

  public OnChanged(state: WorkspaceOriginState) {
    (this.Changed ??= new Set<WorkspaceOriginState>()).add(state)
  }
}
