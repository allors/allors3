import { DatabaseOriginState } from '../originstate/database-origin-state';
import { Strategy } from '../strategy';

export class PushToDatabaseTracker {
  created: Set<Strategy>;

  changed: Set<DatabaseOriginState>;

  onCreated(strategy: Strategy) {
    (this.created ??= new Set<Strategy>()).add(strategy);
  }

  onChanged(state: DatabaseOriginState) {
    if (!state.strategy.isNew) {
      (this.changed ??= new Set<DatabaseOriginState>()).add(state);
    }
  }
}
