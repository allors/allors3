import { RoleDependency, RoleType, Composite } from '@allors/workspace/meta/system';

export class LazyRoleDependency implements RoleDependency {
  readonly kind = 'RoleDependency';
  
  constructor(public readonly objectType: Composite, public readonly roleType: RoleType) {}
}
