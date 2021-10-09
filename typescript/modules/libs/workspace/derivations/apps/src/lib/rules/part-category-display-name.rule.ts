import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PartCategory } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class PartCategoryDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.PartCategory.PrimaryParent,
      },
    ];

    this.dependencies = [d(m.PartCategory, (v) => v.PrimaryParent)];
  }

  derive(cycle: ICycle, matches: PartCategory[]) {
    for (const match of matches) {
      const selfAndPrimaryAncestors = [match];
      let ancestor: PartCategory | null = match;
      while (ancestor != null && selfAndPrimaryAncestors.indexOf(ancestor) < 0) {
        selfAndPrimaryAncestors.push(ancestor);
        ancestor = ancestor.PrimaryParent;
      }

      selfAndPrimaryAncestors.reverse();
      match.DisplayName = selfAndPrimaryAncestors.map((v) => v.Name).join('/');
    }
  }
}
