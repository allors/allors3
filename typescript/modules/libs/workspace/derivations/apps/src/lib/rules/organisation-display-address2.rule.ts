import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '@allors/workspace/domain/default';

export class OrganisationDisplayAddress2Rule implements IRule {
  id: 'cbfbe6eab29e4f72a2e68c2d66c7f61c';
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
        roleType: m.PostalAddress.PostalCode,
        tree: t.ContactMechanism({
          PartiesWhereGeneralCorrespondence: {},
        }),
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Locality,
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
        match.DisplayAddress2 = `${postalAddress.PostalCode} ${postalAddress.Locality}`;
      }
    
      match.DisplayAddress2 = '';
    }
  }
}
