import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { EmailAddress, Person } from '@allors/workspace/domain/default';

export class PersonDisplayEmailRule implements IRule {
  id: 'ee5e33e6c1aa48749488d36ceb8b8fd1';
  patterns: IPattern[];
  m: M;

  constructor(m: M) {
    this.m = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Person.FirstName,
      },
      {
        kind: 'RolePattern',
        roleType: m.Person.LastName,
      },
    ];
  }

  derive(cycle: ICycle, matches: Person[]) {
    for (const match of matches) {
      const emailAddresses = match.PartyContactMechanisms.filter((v) => v.ContactMechanism?.strategy.cls === this.m.EmailAddress)
      .map((v) => {
        const emailAddress = v.ContactMechanism as EmailAddress;
        return emailAddress.ElectronicAddressString;
      })
      .filter((v) => v) as string[];
  
    match.DisplayEmail = emailAddresses.join(', ');    }
  }
}
