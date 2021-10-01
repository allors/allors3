import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation,  } from '@allors/workspace/domain/default';

export class OrganisationDisplayClassificationRule implements IRule {
  id= '77a836adb91a425c8556364bec3002b1';
  patterns: IPattern[];
  m: M;

  constructor(m: M) {
    this.m = m;
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Organisation.CustomClassifications,
      },
      {
        kind: 'RolePattern',
        roleType: m.CustomOrganisationClassification.Name,
        tree: t.CustomOrganisationClassification({
          OrganisationsWhereCustomClassification: {},
        }),
      },
    ];
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const match of matches) {
      match.DisplayClassification = match.CustomClassifications.map((w) => w.Name).join(', ');    }
  }
}
