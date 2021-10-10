import { Dependency } from '@allors/workspace/meta/system';
import { Organisation } from '@allors/workspace/domain/default';
import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

export class OrganisationDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Organisation.Owner,
      },
      {
        kind: 'RolePattern',
        roleType: m.Person.FirstName,
        tree: t.Person({
          OrganisationsWhereOwner: {},
        }),
      },
    ];

    this.dependencies = [d(m.Organisation, (v) => v.Owner)];
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const organisation of matches) {
      organisation.DisplayName = `${organisation.Name} owned by ${organisation.Owner.FirstName}`;
    }
  }
}
