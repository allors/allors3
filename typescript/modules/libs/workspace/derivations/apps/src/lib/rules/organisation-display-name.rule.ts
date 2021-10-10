import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class OrganisationDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [p(m.Organisation, (v) => v.Name)];
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const match of matches) {
      match.DisplayName = match.Name ?? 'N/A';
    }
  }
}
