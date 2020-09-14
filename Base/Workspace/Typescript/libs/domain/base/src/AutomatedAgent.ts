import { Database } from '@allors/workspace/system';
import { Meta } from '@allors/meta/generated';
import { AutomatedAgent } from '@allors/domain/generated';
import { assert } from '@allors/meta/system';

export function extendAutomatedAgent(database: Database) {

  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.AutomatedAgent);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: AutomatedAgent): string {
      return this.UserName ?? 'N/A';
    },
  });
};
