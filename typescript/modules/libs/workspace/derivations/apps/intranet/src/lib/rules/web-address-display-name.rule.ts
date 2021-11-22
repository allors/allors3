import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WebAddress } from '@allors/workspace/domain/default';

export class WebAddressDisplayNameRule implements IRule<WebAddress> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.WebAddress;
    this.roleType = m.WebAddress.DisplayName;
  }

  derive(webAddress: WebAddress) {
    return webAddress.ElectronicAddressString ?? 'N/A';
  }
}
