import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { PostalAddress } from '@allors/default/workspace/domain';

export class PostalAddressDisplayNameRule implements IRule<PostalAddress> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.objectType = m.PostalAddress;
    this.roleType = m.PostalAddress.DisplayName;
    this.dependencies = [d(m.PostalAddress, (v) => v.Country)];
  }

  derive(postalAddress: PostalAddress) {
    const parts: string[] = [postalAddress.Address1,
      postalAddress.Address2,
      postalAddress.Address3,
      postalAddress.PostalCode,
      postalAddress.Locality,
      postalAddress.Country?.Name];

    return parts.filter(s => !!s).toString();
  }
}
