import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';

export class OrganisationDisplayNameRule implements IRule<Organisation> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.roleType = m.Organisation.DisplayName;
  }

  derive(organisation: Organisation) {
    organisation.DisplayName = organisation.Name ?? 'N/A';
  }
}
