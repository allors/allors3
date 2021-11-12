import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
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
    let name = null;
    if (person.FirstName || person.LastName) {
      if (person.FirstName) {
        name = person.FirstName;
      }

      if (person.MiddleName) {
        if (name != null) {
          name += ' ' + person.MiddleName;
        } else {
          name = person.MiddleName;
        }
      }

      if (person.LastName) {
        if (name != null) {
          name += ' ' + person.LastName;
        } else {
          name = person.LastName;
        }
      }
    } else if (person.UserName) {
      name = person.UserName;
    } else {
      name = 'N/A';
    }

    return name;
  }
}
