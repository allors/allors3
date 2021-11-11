import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { Organisation } from '@allors/workspace/domain/default';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

export class OrganisationDisplayNameRule implements IRule<Organisation> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;
    this.roleType = m.Organisation.DisplayName;
    this.dependencies = [d(m.Organisation, (v) => v.Owner)];
  }

  derive(organisation: Organisation) {
    return `${organisation.Name} owned by ${organisation.Owner.FirstName}`;
  }
}
