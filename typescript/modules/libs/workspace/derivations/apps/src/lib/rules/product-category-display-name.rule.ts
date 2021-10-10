import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { ProductCategory } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class ProductCategoryDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [p(m.ProductCategory, (v) => v.PrimaryParent)];

    this.dependencies = [d(m.ProductCategory, (v) => v.PrimaryParent)];
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
