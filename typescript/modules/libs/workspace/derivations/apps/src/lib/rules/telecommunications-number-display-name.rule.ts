import { inlineLists } from 'common-tags';

import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { TelecommunicationsNumber } from '@allors/workspace/domain/default';

export class TelecommunicationsNumberDisplayNameRule implements IRule {
  id= 'c44bbca39c4f4ec898a7f47751dd63c1';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.TelecommunicationsNumber.CountryCode,
      },
      {
        kind: 'RolePattern',
        roleType: m.TelecommunicationsNumber.AreaCode,
      },
      {
        kind: 'RolePattern',
        roleType: m.TelecommunicationsNumber.ContactNumber,
      },
    ];
  }

  derive(cycle: ICycle, matches: TelecommunicationsNumber[]) {
    for (const match of matches) {
      match.DisplayName = inlineLists`${[match.CountryCode, match.AreaCode, match.ContactNumber].filter((v) => v)}`;
    }
  }
}
