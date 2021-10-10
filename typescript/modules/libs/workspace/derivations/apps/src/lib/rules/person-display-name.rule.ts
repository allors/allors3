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
      person.DisplayName = `${person.FirstName} ${person.LastName}`;
    }
  }
}
