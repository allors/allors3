import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '@allors/workspace/domain/default';

export class OrganisationDisplayAddressRule implements IRule {
  id: '456d078529924e8c9e9f62e60c756325';
  patterns: IPattern[];
  m: M;

  constructor(m: M) {
    this.m = m;
    const { treeBuilder: t } = m;

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
