import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '@allors/workspace/domain/default';

export class OrganisationDisplayAddress2Rule implements IRule<Organisation> {
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { treeBuilder: t, dependency: d } = m;

    this.roleType = m.Organisation.DisplayAddress2;

    this.dependencies = [d(m.Organisation, (v) => v.GeneralCorrespondence)];
  }

  derive(organisation: Organisation) {
    if (organisation.GeneralCorrespondence && organisation.GeneralCorrespondence.strategy.cls === this.m.PostalAddress) {
      const postalAddress = organisation.GeneralCorrespondence as PostalAddress;
      organisation.DisplayAddress2 = `${postalAddress.PostalCode} ${postalAddress.Locality}`;
    }

    organisation.DisplayAddress2 = '';
  }
}
