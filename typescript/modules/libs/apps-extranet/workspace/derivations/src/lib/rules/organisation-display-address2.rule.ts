import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '@allors/workspace/domain/default';

export class OrganisationDisplayAddress2Rule implements IRule<Organisation> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.objectType = m.Organisation;
    this.roleType = m.Organisation.DisplayAddress2;

    this.dependencies = [d(m.Organisation, (v) => v.GeneralCorrespondence)];
  }

  derive(organisation: Organisation) {
    if (organisation.GeneralCorrespondence && organisation.GeneralCorrespondence.strategy.cls === this.m.PostalAddress) {
      const postalAddress = organisation.GeneralCorrespondence as PostalAddress;
      return `${postalAddress.PostalCode} ${postalAddress.Locality}`;
    }

    return '';
  }
}
