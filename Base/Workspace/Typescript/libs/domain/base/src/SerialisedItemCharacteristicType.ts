import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { SerialisedItemCharacteristicType } from '@allors/domain/generated';
import { Database } from '@allors/workspace/system';

export function extendSerialisedItemCharacteristicType(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.SerialisedItemCharacteristicType);
  assert(cls);

  Object.defineProperties(cls.prototype, {
    displayName: {
      configurable: true,
      get(this: SerialisedItemCharacteristicType): string {
        return (this.UnitOfMeasure ? this.Name + ' (' + this.UnitOfMeasure.Abbreviation + ')' : this.Name) ?? '';
      },
    },
  });
}
