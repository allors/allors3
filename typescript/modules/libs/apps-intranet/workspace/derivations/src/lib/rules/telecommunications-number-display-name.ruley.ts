import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { TelecommunicationsNumber } from '@allors/default/workspace/domain';

export class TelecommunicationsNumberDisplayNameRule
  implements IRule<TelecommunicationsNumber>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;

    this.objectType = m.TelecommunicationsNumber;
    this.roleType = m.TelecommunicationsNumber.DisplayName;
  }

  derive(telecommunicationsNumber: TelecommunicationsNumber) {
    const parts: string[] = [
      telecommunicationsNumber.CountryCode,
      telecommunicationsNumber.AreaCode,
      telecommunicationsNumber.ContactNumber,
    ];

    return parts.filter((s) => !!s).toString();
  }
}
