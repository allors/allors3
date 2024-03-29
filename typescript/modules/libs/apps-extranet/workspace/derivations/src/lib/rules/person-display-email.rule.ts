import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { EmailAddress, Person } from '@allors/default/workspace/domain';

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

    this.dependencies = [d(m.Person, (v) => v.CurrentPartyContactMechanisms)];
  }

  derive(person: Person) {
    const emailAddresses = person.CurrentPartyContactMechanisms.filter(
      (v) => v.ContactMechanism?.strategy.cls === this.m.EmailAddress
    )
      .map((v) => {
        const emailAddress = v.ContactMechanism as EmailAddress;
        return emailAddress.ElectronicAddressString;
      })
      .filter((v) => v) as string[];

    return emailAddresses.join(', ');
  }
}
