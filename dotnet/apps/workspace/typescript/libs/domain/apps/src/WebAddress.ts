import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { WebAddress } from '@allors/domain/generated';
import { Database } from '@allors/workspace/core';


export function extendWebAddress(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.WebAddress);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: WebAddress) {
      return this.ElectronicAddressString ?? 'N/A';
    },
  });
}
