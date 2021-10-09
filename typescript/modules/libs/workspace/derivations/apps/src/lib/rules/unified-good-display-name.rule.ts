import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { UnifiedGood } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class UnifiedGoodDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.ProductCategory.DisplayName,
      },
    ];

    this.dependencies = [d(m.UnifiedGood, (v) => v.ProductCategoriesWhereProduct)];
  }

  derive(cycle: ICycle, matches: UnifiedGood[]) {
    for (const match of matches) {
      match.DisplayName = match.ProductCategoriesWhereProduct.map((v) => v.DisplayName).join(', ');
    }
  }
}
