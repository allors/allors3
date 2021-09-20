import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { PostalAddress } from '@allors/domain/generated';
import { oneLine, stripIndents, inlineLists } from 'common-tags';
import { Database } from '@allors/workspace/core';

export function extendPostalAddress(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.PostalAddress);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: PostalAddress) {
      return stripIndents`
      ${[this.Address1, this.Address2, this.Address3]
        .filter((v) => v)
        .map((v) => oneLine`${v}`)}
      ${inlineLists`${[this.PostalCode, this.Locality].filter((v) => v)}`}
      ${this.Country?.Name ?? ''}
      `;
    },
  });
}
