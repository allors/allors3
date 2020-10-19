import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { SerialisedInventoryItem } from '@allors/domain/generated';
import { Database } from '@allors/workspace/core';


export function extendSerialisedInventoryItem(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.SerialisedInventoryItem);
  assert(cls);

  Object.defineProperty(cls, 'facilityName', {
    configurable: true,
    get(this: SerialisedInventoryItem): string {
      return this.Facility?.Name ?? '';
    },
  });
}
