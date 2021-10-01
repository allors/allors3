import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WebAddress } from '@allors/workspace/domain/default';

export class WebAddressDisplayNameRule implements IRule {
  id= '2bca556204f7448ca779e2c5141d98fb';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

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
