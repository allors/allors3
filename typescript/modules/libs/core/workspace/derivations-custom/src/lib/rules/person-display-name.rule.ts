import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Person } from '@allors/workspace/domain/default';

export class PersonDisplayNameRule implements IRule<Person> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.Person;
    this.roleType = m.Person.DisplayName;
  }

  derive(person: Person) {
    return `${person.FirstName} ${person.LastName}`;
  }
}
