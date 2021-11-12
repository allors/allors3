import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { UnifiedGood } from '@allors/workspace/domain/default';

export class UnifiedGoodDisplayNameRule implements IRule<UnifiedGood> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.UnifiedGood;
    this.roleType = m.UnifiedGood.DisplayName;

    this.dependencies = [d(m.UnifiedGood, (v) => v.ProductCategoriesWhereProduct)];
  }

  derive(unifiedGood: UnifiedGood) {
    return unifiedGood.ProductCategoriesWhereProduct?.map((v) => v.DisplayName).join(', ');
  }
}
