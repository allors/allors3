import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Organisation, PostalAddress } from '@allors/default/workspace/domain';

export class OrganisationDisplayAddress3Rule implements IRule<Organisation> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.objectType = m.Organisation;
    this.roleType = m.Organisation.DisplayAddress3;

    this.dependencies = [d(m.Organisation, (v) => v.GeneralCorrespondence)];
  }

  derive(organisation: Organisation) {
    if (
      organisation.GeneralCorrespondence &&
      organisation.GeneralCorrespondence.strategy.cls === this.m.PostalAddress
    ) {
      const postalAddress = organisation.GeneralCorrespondence as PostalAddress;
      return `${postalAddress.Country ? postalAddress.Country.Name : ''}`;
    }

    return '';
  }
}
