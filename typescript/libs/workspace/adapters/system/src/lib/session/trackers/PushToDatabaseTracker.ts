import { DatabaseOriginState } from '../originstate/DatabaseOriginState';
import { Strategy } from '../Strategy';

export class PushToDatabaseTracker {
  Created: Set<Strategy>;

  Changed: Set<DatabaseOriginState>;

  OnCreated(strategy: Strategy) {
    (this.Created ??= new Set<Strategy>()).add(strategy);
  }

  OnChanged(state: DatabaseOriginState) {
    if (!state.strategy.isNew) {
      (this.Changed ??= new Set<DatabaseOriginState>()).add(state);
    }
  }
}
