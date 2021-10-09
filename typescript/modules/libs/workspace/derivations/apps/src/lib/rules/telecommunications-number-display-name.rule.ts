import { inlineLists } from 'common-tags';

import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { TelecommunicationsNumber } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class TelecommunicationsNumberDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
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
