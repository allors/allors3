import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PartCategory } from '@allors/workspace/domain/default';

export class PartCategoryDisplayNameRule implements IRule<PartCategory> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.roleType = m.PartCategory.DisplayName;

    this.dependencies = [d(m.PartCategory, (v) => v.PrimaryParent)];
  }

  derive(partCategory: PartCategory) {
    const selfAndPrimaryAncestors = [partCategory];
    let ancestor: PartCategory | null = partCategory;
    while (ancestor != null && selfAndPrimaryAncestors.indexOf(ancestor) < 0) {
      selfAndPrimaryAncestors.push(ancestor);
      ancestor = ancestor.PrimaryParent;
    }

    selfAndPrimaryAncestors.reverse();
    partCategory.DisplayName = selfAndPrimaryAncestors.map((v) => v.Name).join('/');
  }
}
