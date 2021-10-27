import { Composite, CompositeExtension } from './composite';
import { RoleType } from './role-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface ClassExtension extends CompositeExtension {}

export interface Class extends Composite {
  readonly kind: 'Class';
  _: ClassExtension;
  overriddenRequiredRoleTypes: RoleType[];
  requiredRoleTypes: Set<RoleType>;
}
