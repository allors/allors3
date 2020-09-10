import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { SerialisedInventoryItem } from '@allors/session/generated';
import { Database } from '@allors/domain/system';


export function extendSerialisedInventoryItem(workspace: Database) {
  const m = workspace.metaPopulation as Meta;
  const cls = workspace.constructorByObjectType.get(m.SerialisedInventoryItem);
  assert(cls);

  Object.defineProperty(cls, 'facilityName', {
    configurable: true,
    get(this: SerialisedInventoryItem): string {
      return this.Facility?.Name ?? '';
    },
  });
}
