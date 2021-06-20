import { AssociationType,  RoleType} from "@allors/workspace/meta/system";
import { ISession } from "./ISession";
import { IStrategy } from "./IStrategy";

export interface IChangeSet {
  Session: ISession;

  Created: Set<IStrategy>;

  Instantiated: Set<IStrategy>;

  associationsByRoleType: Map<RoleType, Set<IStrategy>>;

  rolesByAssociationType: Map<AssociationType, Set<IStrategy>>;
}
