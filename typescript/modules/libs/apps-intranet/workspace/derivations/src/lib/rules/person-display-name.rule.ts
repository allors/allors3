import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Person } from '@allors/default/workspace/domain';

export class PersonDisplayNameRule implements IRule<Person> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;

    this.objectType = m.Person;
    this.roleType = m.Person.DisplayName;
  }

  derive(person: Person) {
    let displayName = `${person.FirstName}`;
   
    if (person.MiddleName) {
      if (displayName) {
        displayName += ` ${person.MiddleName}`;
      } else {
        displayName += `${person.MiddleName}`;
      }
    }
   
    if (person.LastName) {
      if (displayName) {
        displayName += ` ${person.LastName}`;
      } else {
        displayName += `${person.LastName}`;
      }
    }

    return displayName;
  }
}
