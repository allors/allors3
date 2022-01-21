import { IObject } from '@allors/workspace/domain/system';
import { DatabaseOriginState } from '../originstate/database-origin-state';

export class PushToDatabaseTracker {
  created: Set<IObject>;

  changed: Set<DatabaseOriginState>;

  onCreated(strategy: IObject) {
    (this.created ??= new Set<IObject>()).add(strategy);
  }

  onChanged(state: DatabaseOriginState) {
    if (!state.object.strategy.isNew) {
      (this.changed ??= new Set<DatabaseOriginState>()).add(state);
    }
  }

  onDelete(object: IObject) {
    this.created?.delete(object);
  }
}
