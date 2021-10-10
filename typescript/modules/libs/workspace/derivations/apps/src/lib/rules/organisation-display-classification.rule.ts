import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class OrganisationDisplayClassificationRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      p(m.Organisation, (v) => v.CustomClassifications),
      p(
        m.CustomOrganisationClassification,
        (v) => v.Name,
        t.CustomOrganisationClassification({
          OrganisationsWhereCustomClassification: {},
        })
      ),
    ];

    this.dependencies = [d(m.Organisation, (v) => v.CustomClassifications)];
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const match of matches) {
      match.DisplayClassification = match.CustomClassifications.map((w) => w.Name).join(', ');
    }
  }
}
