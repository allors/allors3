import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { WorkEffortPartyAssignment } from '@allors/domain/generated';
import { Database } from '@allors/workspace/core';

export function extendWorkEffortPartyAssignment(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.WorkEffortPartyAssignment);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: WorkEffortPartyAssignment) {
      return this.Party?.displayName ?? 'N/A';
    },
  });
}
