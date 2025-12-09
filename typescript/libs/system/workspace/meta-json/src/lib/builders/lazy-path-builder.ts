import { Path } from '@allors/system/workspace/domain';
import { Composite, MetaPopulation } from '@allors/system/workspace/meta';

export class LazyPathBuilder {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (obj) => {
        const entry = Object.entries(obj).find(([k]) => k !== 'ofType');

        if (!entry) {
          return null;
        }

        const [key, value] = entry;
        const propertyType = composite.propertyTypeByPropertyName.get(key);
        const builder = this[propertyType.objectType.singularName];

        const path: Path = {
          propertyType,
        };

        const ofTypeEntry = Object.entries(obj).find(([k]) => k === 'ofType');
        if (ofTypeEntry) {
          const [, ofType] = ofTypeEntry;
          path.ofType = ofType as Composite;
        }

        const next = builder(value);
        if (next) {
          path.next = next;
        }

        return path;
      };
    }
  }
}
