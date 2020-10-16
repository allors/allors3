import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { EmailAddress } from '@allors/domain/generated';
import { Database } from '@allors/workspace/system';

export function extendEmailAddress(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.EmailAddress);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: EmailAddress) {
      return this.ElectronicAddressString ?? 'N/A';
    },
  });
}
