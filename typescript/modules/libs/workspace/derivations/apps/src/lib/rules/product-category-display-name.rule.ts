import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { ProductCategory } from '@allors/workspace/domain/default';

export class ProductCategoryDisplayNameRule implements IRule {
  id: '431d3b390dc44e88afab436b60d7752f';
  patterns: IPattern[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.ProductCategory.PrimaryParent,
      },
    ];
  }

  derive(cycle: ICycle, matches: ProductCategory[]) {
    for (const match of matches) {
      const selfAndPrimaryAncestors = [match];
      let ancestor: ProductCategory | null = match;
      while (ancestor != null && selfAndPrimaryAncestors.indexOf(ancestor) < 0) {
        selfAndPrimaryAncestors.push(ancestor);
        ancestor = ancestor.PrimaryParent;
      }
    
      selfAndPrimaryAncestors.reverse();
      match.DisplayName = selfAndPrimaryAncestors.map((v) => v.Name).join('/');
    }
  }
}
