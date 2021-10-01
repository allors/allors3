import { stripIndents, oneLine, inlineLists } from 'common-tags';

import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PostalAddress } from '@allors/workspace/domain/default';

export class PostalAddressDisplayNameRule implements IRule {
  id= 'b13f33e4fbf44bb5a2b4cd0c211a5ca2';
  patterns: IPattern[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Address1,
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Address2,
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Address3,
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.PostalCode,
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Locality,
      },
      {
        kind: 'RolePattern',
        roleType: m.PostalAddress.Country,
      },
    ];
  }

  derive(cycle: ICycle, matches: PostalAddress[]) {
    for (const match of matches) {
      match.DisplayName = stripIndents`
      ${[match.Address1, match.Address2, match.Address3].filter((v) => v).map((v) => oneLine`${v}`)}
      ${inlineLists`${[match.PostalCode, match.Locality].filter((v) => v)}`}
      ${match.Country?.Name ?? ''}
      `;    
    }
  }
}
