import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';

export class OrganisationDisplayClassificationRule implements IRule<Organisation> {
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.roleType = m.Organisation.DisplayClassification;

    this.dependencies = [d(m.Organisation, (v) => v.CustomClassifications)];
  }

  derive(match: Organisation) {
    match.DisplayClassification = match.CustomClassifications.map((w) => w.Name).join(', ');
  }
}
