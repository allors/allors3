import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { EmailAddress } from '@allors/workspace/domain/default';

export class EmailAddressDisplayNameRule implements IRule<EmailAddress> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.EmailAddress;
    this.roleType = m.EmailAddress.DisplayName;
  }

  derive(emailAddress: EmailAddress) {
    return emailAddress.ElectronicAddressString ?? 'N/A';
  }
}
