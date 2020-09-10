import { Database } from '@allors/domain/system';
import { Meta } from '@allors/meta/generated';
import { AutomatedAgent } from '@allors/session/generated';
import { assert } from '@allors/meta/system';

export function extendAutomatedAgent(workspace: Database) {

  const m = workspace.metaPopulation as Meta;
  const cls = workspace.constructorByObjectType.get(m.AutomatedAgent);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: AutomatedAgent): string {
      return this.UserName ?? 'N/A';
    },
  });
};
