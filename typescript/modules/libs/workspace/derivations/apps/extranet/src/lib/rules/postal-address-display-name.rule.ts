import { stripIndents, oneLine, inlineLists } from 'common-tags';

import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PostalAddress } from '@allors/workspace/domain/default';

export class PostalAddressDisplayNameRule implements IRule<PostalAddress> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.PostalAddress;
    this.roleType = m.PostalAddress.DisplayName;

    this.dependencies = [d(m.PostalAddress, (v) => v.Country)];
  }

  derive(postalAddress: PostalAddress) {
    return stripIndents`
      ${[postalAddress.Address1, postalAddress.Address2, postalAddress.Address3].filter((v) => v).map((v) => oneLine`${v}`)}
      ${inlineLists`${[postalAddress.PostalCode, postalAddress.Locality].filter((v) => v)}`}
      ${postalAddress.Country?.Name ?? ''}
      `;
  }
}
