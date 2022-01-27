import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Person } from '@allors/default/workspace/domain';

export class PersonFullNameRule implements IRule<Person> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.Person;
    this.roleType = m.Person.FullName;
  }

  derive(person: Person) {
    return `${person.FirstName} ${person.LastName}`;
  }
}
