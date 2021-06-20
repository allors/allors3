import { AssociationType, RoleType } from '@allors/workspace/meta/system';
import { Strategy } from '../Strategy';

export /* sealed */ class ChangeSetTracker {
  Created: Set<IStrategy>;
  Instantiated: Set<IStrategy>;
  DatabaseOriginStates: Set<DatabaseOriginState>;
  WorkspaceOriginStates: Set<WorkspaceOriginState>;

  public OnCreated(strategy: Strategy) {
    (this.Created ??= new Set()).add(strategy);
  }

  public OnInstantiated(strategy: Strategy) {
    (this.Instantiated ??= new Set()).add(strategy);
  }

  public OnChanged(state: DatabaseOriginState) {
    (this.DatabaseOriginStates ??= new Set()).add(state);
  }

  public OnChanged(state: WorkspaceOriginState) {
    (this.WorkspaceOriginStates ??= new Set()).add(state);
  }
}
