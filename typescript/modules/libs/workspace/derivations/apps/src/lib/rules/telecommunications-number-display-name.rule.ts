import { inlineLists } from 'common-tags';

import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { TelecommunicationsNumber } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class TelecommunicationsNumberDisplayNameRule implements IRule {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [p(m.TelecommunicationsNumber, (v) => v.CountryCode), p(m.TelecommunicationsNumber, (v) => v.AreaCode), p(m.TelecommunicationsNumber, (v) => v.ContactNumber)];
  }

  derive(cycle: ICycle, matches: TelecommunicationsNumber[]) {
    for (const match of matches) {
      match.DisplayName = inlineLists`${[match.CountryCode, match.AreaCode, match.ContactNumber].filter((v) => v)}`;
    }
  }
}
