import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';

export class OrganisationDisplayNameRule implements IRule {
  id= 'c2cfecbd3b4f437198c53b9c5b206f0c';
  patterns: IPattern[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Organisation.Name,
      },
    ];
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const match of matches) {
      match.DisplayName = match.Name ?? 'N/A';
    }
  }
}
