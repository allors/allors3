import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';

export class OrganisationDisplayNameRule implements IRule<Organisation> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.Organisation;
    this.roleType = m.Organisation.DisplayName;
  }

  derive(organisation: Organisation) {
    return organisation.Name ?? 'N/A';
  }
}
