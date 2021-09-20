import { Composite } from './Composite';
import { RoleType } from './RoleType';

export interface Class extends Composite {
  readonly kind: 'Class';

  overriddenRequiredRoleTypes: RoleType[];

  overriddenUniqueRoleTypes: RoleType[];

  requiredRoleTypes: Set<RoleType>;

  uniqueRoleTypes: Set<RoleType>;
}
