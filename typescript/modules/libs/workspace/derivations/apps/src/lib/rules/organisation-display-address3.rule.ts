import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '@allors/workspace/domain/default';

export class OrganisationDisplayAddress3Rule implements IRule {
  id= '84d7e301671749959a4acac3d9d145ef';
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
        roleType: m.PostalAddress.Country,
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
        match.DisplayAddress3 = `${postalAddress.Country ? postalAddress.Country.Name : ''}`;
      }
    
      match.DisplayAddress3 = '';
    }
  }
}
