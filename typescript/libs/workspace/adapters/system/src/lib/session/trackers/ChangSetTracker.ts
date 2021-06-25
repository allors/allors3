import { DatabaseOriginState } from '../originstate/DatabaseOriginState';
import { WorkspaceOriginState } from '../originstate/WorkspaceOriginState';
import { Strategy } from '../Strategy';

export class ChangeSetTracker {
  Created: Set<Strategy>;
  Instantiated: Set<Strategy>;
  DatabaseOriginStates: Set<DatabaseOriginState>;
  WorkspaceOriginStates: Set<WorkspaceOriginState>;

  public OnCreated(strategy: Strategy) {
    (this.Created ??= new Set()).add(strategy);
  }

  public OnInstantiated(strategy: Strategy) {
    (this.Instantiated ??= new Set()).add(strategy);
  }

  public OnDatabaseChanged(state: DatabaseOriginState) {
    (this.DatabaseOriginStates ??= new Set()).add(state);
  }

  public OnWorkspaceChanged(state: WorkspaceOriginState) {
    (this.WorkspaceOriginStates ??= new Set()).add(state);
  }
}
