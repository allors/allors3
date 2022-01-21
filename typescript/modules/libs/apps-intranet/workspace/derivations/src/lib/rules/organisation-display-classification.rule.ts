import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';

export class OrganisationDisplayClassificationRule
  implements IRule<Organisation>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.objectType = m.Organisation;
    this.roleType = m.Organisation.DisplayClassification;

    this.dependencies = [d(m.Organisation, (v) => v.CustomClassifications)];
  }

  derive(organisation: Organisation) {
    return organisation.CustomClassifications.map((w) => w.Name).join(', ');
  }
}
