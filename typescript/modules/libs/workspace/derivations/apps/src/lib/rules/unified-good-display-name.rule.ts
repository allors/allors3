import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { UnifiedGood } from '@allors/workspace/domain/default';

export class UnifiedGoodDisplayNameRule implements IRule {
  id: '6f30aa5a695e4dfa8a110dc6748404aa';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.ProductCategory.DisplayName,
      },
    ];
  }

  derive(cycle: ICycle, matches: UnifiedGood[]) {
    for (const match of matches) {
      match.DisplayName = match.ProductCategoriesWhereProduct.map((v) => v.DisplayName).join(', ');
    }
  }
}
