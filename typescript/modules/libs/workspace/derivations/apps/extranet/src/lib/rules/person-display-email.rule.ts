import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { EmailAddress, Person } from '@allors/workspace/domain/default';

export class PersonDisplayEmailRule implements IRule<Person> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    const { dependency: d } = m;
    this.m = m;

    this.objectType = m.Person;
    this.roleType = m.Person.DisplayEmail;

    this.dependencies = [d(m.Person, (v) => v.PartyContactMechanisms)];
  }

  derive(person: Person) {
    const emailAddresses = person.PartyContactMechanisms.filter((v) => v.ContactMechanism?.strategy.cls === this.m.EmailAddress)
      .map((v) => {
        const emailAddress = v.ContactMechanism as EmailAddress;
        return emailAddress.ElectronicAddressString;
      })
      .filter((v) => v) as string[];

    return emailAddresses.join(', ');
  }
}
