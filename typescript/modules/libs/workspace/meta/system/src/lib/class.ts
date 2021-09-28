import { Composite } from './composite';
import { RoleType } from './role-type';

export interface Class extends Composite {
  readonly kind: 'Class';

  overriddenRequiredRoleTypes: RoleType[];

  overriddenUniqueRoleTypes: RoleType[];

  requiredRoleTypes: Set<RoleType>;

  uniqueRoleTypes: Set<RoleType>;
}
