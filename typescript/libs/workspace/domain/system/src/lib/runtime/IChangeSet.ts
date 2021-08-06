import { AssociationType, RoleType } from "@allors/workspace/meta/system";
import { ISession } from "./ISession";
import { IStrategy } from "./IStrategy";

export interface IChangeSet {
  session: ISession;

  created: Readonly<Set<IStrategy>>;

  instantiated: Readonly<Set<IStrategy>>;

  associationsByRoleType: Readonly<Map<RoleType, Set<IStrategy>>>;

  rolesByAssociationType: Readonly<Map<AssociationType, Set<IStrategy>>>;
}
