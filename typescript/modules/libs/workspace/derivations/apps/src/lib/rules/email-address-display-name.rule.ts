import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { EmailAddress } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class EmailAddressDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [p(m.EmailAddress, (v) => v.ElectronicAddressString)];
  }

  derive(cycle: ICycle, matches: EmailAddress[]) {
    for (const match of matches) {
      match.DisplayName = match.ElectronicAddressString ?? 'N/A';
    }
  }
}
