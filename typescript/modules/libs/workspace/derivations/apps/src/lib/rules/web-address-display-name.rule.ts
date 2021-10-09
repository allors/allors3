import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WebAddress } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class WebAddressDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.WebAddress.ElectronicAddressString,
      },
    ];
  }

  derive(cycle: ICycle, matches: WebAddress[]) {
    for (const match of matches) {
      match.DisplayName = match.ElectronicAddressString ?? 'N/A';
    }
  }
}
