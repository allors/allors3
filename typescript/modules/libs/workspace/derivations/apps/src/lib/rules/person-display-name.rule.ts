import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Person } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class PersonDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [p(m.Person, (v) => v.FirstName), p(m.Person, (v) => v.LastName)];
  }

  derive(cycle: ICycle, matches: Person[]) {
    for (const person of matches) {
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

      person.DisplayName = name;
    }
  }
}
