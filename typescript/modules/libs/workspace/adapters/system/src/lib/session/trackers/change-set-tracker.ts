import { IObject } from '@allors/workspace/domain/system';
import { DatabaseOriginState } from '../originstate/database-origin-state';
import { WorkspaceOriginState } from '../originstate/workspace-origin-state';

export class ChangeSetTracker {
  created: Set<IObject>;
  instantiated: Set<IObject>;
  databaseOriginStates: Set<DatabaseOriginState>;
  workspaceOriginStates: Set<WorkspaceOriginState>;

  public onCreated(object: IObject) {
    (this.created ??= new Set()).add(object);
  }

  public onInstantiated(object: IObject) {
    (this.instantiated ??= new Set()).add(object);
  }

  public onDatabaseChanged(state: DatabaseOriginState) {
    (this.databaseOriginStates ??= new Set()).add(state);
  }

  public onWorkspaceChanged(state: WorkspaceOriginState) {
    (this.workspaceOriginStates ??= new Set()).add(state);
  }
}
