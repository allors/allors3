import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { Person } from '@allors/workspace/domain/core';
import { M } from '@allors/workspace/meta/core';

export class FullNameRule implements IRule {
  id: '7A62C83563AF4E989BB8BF24A9CB7CE7';
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
      person.CustomFullName = `${person.FirstName} ${person.LastName}`;
    }
  }
}
