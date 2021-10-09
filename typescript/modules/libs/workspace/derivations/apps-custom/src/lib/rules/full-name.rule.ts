import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Person } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class FullNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Person.FirstName,
      },
    ];
  }
 
  derive(cycle: ICycle, matches: Person[]) {
    for (const person of matches) {
      //person.FullName = `${person.FirstName} ${person.LastName}`;
    }
  }
}
