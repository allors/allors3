import { inlineLists } from 'common-tags';

import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { TelecommunicationsNumber } from '@allors/workspace/domain/default';

export class TelecommunicationsNumberDisplayNameRule implements IRule<TelecommunicationsNumber> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.TelecommunicationsNumber;
    this.roleType = m.TelecommunicationsNumber.DisplayName;
  }

  derive(match: TelecommunicationsNumber) {
    return inlineLists`${[match.CountryCode, match.AreaCode, match.ContactNumber].filter((v) => v)}`;
  }
}
