import { IObject } from '@allors/system/workspace/domain';
import { DatabaseOriginState } from '../originstate/database-origin-state';

export class ChangeSetTracker {
  created: Set<IObject>;
  databaseOriginStates: Set<DatabaseOriginState>;

  public onCreated(object: IObject) {
    (this.created ??= new Set()).add(object);
  }

  public onDatabaseChanged(state: DatabaseOriginState) {
    (this.databaseOriginStates ??= new Set()).add(state);
  }

  onDelete(object: IObject) {
    this.created?.delete(object);
  }
}
