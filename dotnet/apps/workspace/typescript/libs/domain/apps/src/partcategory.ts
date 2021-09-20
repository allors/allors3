import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { PartCategory } from '@allors/domain/generated';
import { Database } from '@allors/workspace/core';

export function extendPartCategory(database: Database) {

  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.PartCategory);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: PartCategory): string {

      const selfAndPrimaryAncestors = [this];
      let ancestor: PartCategory | null = this;
      while (ancestor != null && selfAndPrimaryAncestors.indexOf(ancestor) < 0) {
        selfAndPrimaryAncestors.push(ancestor);
        ancestor = ancestor.PrimaryParent;
      }

      selfAndPrimaryAncestors.reverse();
      const displayName = selfAndPrimaryAncestors.map(v => v.Name).join('/');
      return displayName;
    },
  });

};
