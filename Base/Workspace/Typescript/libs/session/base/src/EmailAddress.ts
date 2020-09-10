import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { EmailAddress } from '@allors/session/generated';
import { Database } from '@allors/domain/system';

export function extendEmailAddress(workspace: Database) {
  const m = workspace.metaPopulation as Meta;
  const cls = workspace.constructorByObjectType.get(m.EmailAddress);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: EmailAddress) {
      return this.ElectronicAddressString ?? 'N/A';
    },
  });
}
