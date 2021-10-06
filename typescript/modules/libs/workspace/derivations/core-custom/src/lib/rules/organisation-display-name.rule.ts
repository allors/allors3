import { Organisation } from '@allors/workspace/domain/default';
import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

export class OrganisationDisplayNameRule implements IRule {
  id = '000c900fc851453285519f3601758e6f';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

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
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const organisation of matches) {
      organisation.DisplayName = `${organisation.Name} owned by ${organisation.Owner.FirstName}`;
    }
  }
}
