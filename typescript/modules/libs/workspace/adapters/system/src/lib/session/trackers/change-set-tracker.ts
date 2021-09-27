import { IStrategy } from '@allors/workspace/domain/system';
import { DatabaseOriginState } from '../originstate/database-origin-state';
import { WorkspaceOriginState } from '../originstate/workspace-origin-state';
import { Strategy } from '../strategy';

export class ChangeSetTracker {
  created: Set<IStrategy>;
  instantiated: Set<IStrategy>;
  databaseOriginStates: Set<DatabaseOriginState>;
  workspaceOriginStates: Set<WorkspaceOriginState>;

  public onCreated(strategy: Strategy) {
    (this.created ??= new Set()).add(strategy);
  }

  public onInstantiated(strategy: Strategy) {
    (this.instantiated ??= new Set()).add(strategy);
  }

  public onDatabaseChanged(state: DatabaseOriginState) {
    (this.databaseOriginStates ??= new Set()).add(state);
  }

  public onWorkspaceChanged(state: WorkspaceOriginState) {
    (this.workspaceOriginStates ??= new Set()).add(state);
  }
}
