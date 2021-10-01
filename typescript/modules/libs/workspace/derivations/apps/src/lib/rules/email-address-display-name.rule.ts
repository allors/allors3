import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { EmailAddress } from '@allors/workspace/domain/default';

export class EmailAddressDisplayNameRule implements IRule {
  id= '75878728ad114ea6bda75e71abe96e3d';
  patterns: IPattern[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.EmailAddress.ElectronicAddressString,
      },
    ];
  }

  derive(cycle: ICycle, matches: EmailAddress[]) {
    for (const match of matches) {
      match.DisplayName = match.ElectronicAddressString ?? 'N/A';
    }
  }
}
