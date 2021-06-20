import { AssociationType, RoleType } from '@allors/workspace/meta/system';
import { Strategy } from '../Strategy';

export /* sealed */ class PushToDatabaseTracker {

  public get Created(): ISet<Strategy> {
  }
  public set Created(value: ISet<Strategy>)  {
  }

  public get Changed(): ISet<DatabaseOriginState> {
  }
  public set Changed(value: ISet<DatabaseOriginState>)  {
  }

  public OnCreated(strategy: Strategy) {
  }

  public OnChanged(state: DatabaseOriginState) {
      if (!state.Strategy.IsNew) {
          new HashSet<DatabaseOriginState>();
          Add(state);
      }

  }
}
