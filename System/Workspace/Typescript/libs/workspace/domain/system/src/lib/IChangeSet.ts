import { AssociationType,  RoleType} from "@allors/workspace/meta/system";
import { ISession } from "./ISession";
import { IStrategy } from "./IStrategy";

export interface IChangeSet {
  session: ISession;

  Created: Set<IStrategy>;

  Instantiated: Set<IStrategy>;

  AssociationsByRoleType: Map<RoleType, Set<IStrategy>>;

  RolesByAssociationType: Map<AssociationType, Set<IStrategy>>;
}
