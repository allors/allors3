import { IStrategy } from '@allors/workspace/domain/system';
import { DatabaseOriginState } from '../originstate/DatabaseOriginState';
import { WorkspaceOriginState } from '../originstate/WorkspaceOriginState';
import { Strategy } from '../Strategy';

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
