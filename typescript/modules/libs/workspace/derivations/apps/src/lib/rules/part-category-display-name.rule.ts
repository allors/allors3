import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PartCategory } from '@allors/workspace/domain/default';

export class PartCategoryDisplayNameRule implements IRule {
  id= '354610b3405d410dbec4e2322bb3149e';
  patterns: IPattern[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.PartCategory.PrimaryParent,
      },
    ];
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
