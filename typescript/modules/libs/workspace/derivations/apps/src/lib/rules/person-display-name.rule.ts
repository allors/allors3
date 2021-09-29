import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Party } from '@allors/workspace/domain/default';

export class PartyDisplayNameRule implements IRule {
  id: '7A62C83563AF4E989BB8BF24A9CB7CE7';
  patterns: IPattern[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Person.FirstName,
      },
      {
        kind: 'RolePattern',
        roleType: m.Person.LastName,
      },
      {
        kind: 'RolePattern',
        roleType: m.Organisation.Name,
      },
    ];
  }

  derive(cycle: ICycle, matches: Party[]) {
    for (const person of matches) {
      person.SessionFullName = `${person.FirstName} ${person.LastName}`;
    }
  }
}
