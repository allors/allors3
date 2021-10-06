import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Person } from '@allors/workspace/domain/default';

export class FullNameRule implements IRule {
  id = '27844d1bb65f4756b361fe8b8490d953';
  patterns: IPattern[];

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
