import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Person } from '@allors/workspace/domain/default';

export class FullNameRule implements IRule {
  id = '379b8ec6b1f847a990e76cb55d6129a5';
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
