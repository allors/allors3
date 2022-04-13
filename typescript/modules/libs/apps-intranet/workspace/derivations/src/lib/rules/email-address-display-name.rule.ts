import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { EmailAddress } from '@allors/default/workspace/domain';

export class EmailAddressDisplayNameRule implements IRule<EmailAddress> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;

    this.objectType = m.EmailAddress;
    this.roleType = m.EmailAddress.DisplayName;
  }

  derive(emailAddress: EmailAddress) {
        return emailAddress.ElectronicAddressString ?? "N/A";
  }
}
