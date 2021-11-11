import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '@allors/workspace/domain/default';

export class OrganisationDisplayAddress3Rule implements IRule<Organisation> {
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.roleType = m.Organisation.DisplayAddress3;

    this.dependencies = [d(m.Organisation, (v) => v.GeneralCorrespondence)];
  }

  derive(match: Organisation) {
    if (match.GeneralCorrespondence && match.GeneralCorrespondence.strategy.cls === this.m.PostalAddress) {
      const postalAddress = match.GeneralCorrespondence as PostalAddress;
      match.DisplayAddress3 = `${postalAddress.Country ? postalAddress.Country.Name : ''}`;
    }

    match.DisplayAddress3 = '';
  }
}
