import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';

export class OrganisationDisplayNameRule implements IRule<Organisation> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;

    this.objectType = m.Organisation;
    this.roleType = m.Organisation.DisplayName;
  }

  derive(organisation: Organisation) {

    return organisation.Name ?? "N/A";
  }
}
