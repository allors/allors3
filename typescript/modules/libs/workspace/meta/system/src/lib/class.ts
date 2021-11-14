import { Composite } from './composite';
import { RoleType } from './role-type';

export interface Class extends Composite {
  readonly kind: 'Class';
  _: unknown;
  overriddenRequiredRoleTypes: RoleType[];
  requiredRoleTypes: Set<RoleType>;
}
