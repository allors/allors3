import { AssociationType, RoleType } from "@allors/workspace/meta/system";
import { ISession } from "./isession";
import { IStrategy } from "./istrategy";

export interface IChangeSet {
  session: ISession;

  created: Set<IStrategy>;

  instantiated: Set<IStrategy>;

  associationsByRoleType: Map<RoleType, Set<IStrategy>>;

  rolesByAssociationType: Map<AssociationType, Set<IStrategy>>;
}
