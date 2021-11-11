import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { EmailAddress } from '../../../../../domain/apps/src/lib/generated/EmailAddress.g';

export class EmailAddressDisplayNameRule implements IRule<EmailAddress> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.roleType = m.EmailAddress.DisplayName;
  }

  derive(emailAddress: EmailAddress) {
    emailAddress.DisplayName = emailAddress.ElectronicAddressString ?? 'N/A';
  }
}
