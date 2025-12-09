import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';

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
