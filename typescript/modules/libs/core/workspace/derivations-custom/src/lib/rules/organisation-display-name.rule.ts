import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

export class OrganisationDisplayNameRule implements IRule<Organisation> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.Organisation;
    this.roleType = m.Organisation.DisplayName;

    this.dependencies = [d(m.Organisation, (v) => v.Owner)];
  }

  derive(organisation: Organisation) {
    return `${organisation.Name} owned by ${organisation.Owner.FirstName}`;
  }
}
