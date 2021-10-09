import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class OrganisationDisplayAddressRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Organisation.GeneralCorrespondence,
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Address1,
        tree: t.ContactMechanism({
          PartiesWhereGeneralCorrespondence: {},
        }),
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Address2,
        tree: t.ContactMechanism({
          PartiesWhereGeneralCorrespondence: {},
        }),
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Address3,
        tree: t.ContactMechanism({
          PartiesWhereGeneralCorrespondence: {},
        }),
      },
    ];

    this.dependencies = [d(m.Organisation, (v) => v.GeneralCorrespondence)];
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const match of matches) {
      if (match.GeneralCorrespondence && match.GeneralCorrespondence.strategy.cls === this.m.PostalAddress) {
        const postalAddress = match.GeneralCorrespondence as PostalAddress;
        match.DisplayAddress = `${postalAddress.Address1 ? postalAddress.Address1 : ''} ${postalAddress.Address2 ? postalAddress.Address2 : ''} ${postalAddress.Address3 ? postalAddress.Address3 : ''}`;
      }

      match.DisplayAddress = '';
    }
  }
}
