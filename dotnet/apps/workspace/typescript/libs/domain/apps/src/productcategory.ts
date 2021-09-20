import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { ProductCategory } from '@allors/domain/generated';
import { Database } from '@allors/workspace/core';

export function extendProductCategory(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.ProductCategory);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: ProductCategory): string {
      const selfAndPrimaryAncestors = [this];
      let ancestor: ProductCategory | null = this;
      while (
        ancestor != null &&
        selfAndPrimaryAncestors.indexOf(ancestor) < 0
      ) {
        selfAndPrimaryAncestors.push(ancestor);
        ancestor = ancestor.PrimaryParent;
      }

      selfAndPrimaryAncestors.reverse();
      const displayName = selfAndPrimaryAncestors.map((v) => v.Name).join('/');
      return displayName;
    },
  });
}
