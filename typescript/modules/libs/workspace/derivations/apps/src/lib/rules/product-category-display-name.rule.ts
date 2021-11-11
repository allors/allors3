import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { ProductCategory } from '@allors/workspace/domain/default';

export class ProductCategoryDisplayNameRule implements IRule<ProductCategory> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.roleType = m.ProductCategory.DisplayName;

    this.dependencies = [d(m.ProductCategory, (v) => v.PrimaryParent)];
  }

  derive(productCategory: ProductCategory) {
    const selfAndPrimaryAncestors = [productCategory];
    let ancestor: ProductCategory | null = productCategory;
    while (ancestor != null && selfAndPrimaryAncestors.indexOf(ancestor) < 0) {
      selfAndPrimaryAncestors.push(ancestor);
      ancestor = ancestor.PrimaryParent;
    }

    selfAndPrimaryAncestors.reverse();
    productCategory.DisplayName = selfAndPrimaryAncestors.map((v) => v.Name).join('/');
  }
}
