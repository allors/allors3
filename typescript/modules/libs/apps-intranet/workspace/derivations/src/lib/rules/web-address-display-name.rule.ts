import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { WebAddress } from '@allors/default/workspace/domain';

export class WebAddressDisplayNameRule implements IRule<WebAddress> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;

    this.objectType = m.WebAddress;
    this.roleType = m.WebAddress.DisplayName;
  }

  derive(webAddress: WebAddress) {
    return webAddress.ElectronicAddressString ?? 'N/A';
  }
}
