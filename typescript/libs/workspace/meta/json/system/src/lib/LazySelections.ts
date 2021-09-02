import { Select } from '@allors/workspace/domain/system';
import { MetaPopulation, PropertyType } from '@allors/workspace/meta/system';
import { Node } from '@allors/workspace/domain/system';
import { LazyTrees } from './LazyTrees';

export class LazySelections {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (obj) => {
        let propertyType: PropertyType;
        let include: Node[];
        let next: Select;

        for (const [key, value] of Object.entries(obj)) {
          if ('include' === key.valueOf()) {
            const trees = metaPopulation['trees'] as LazyTrees;
            include = trees[composite.singularName](value);
            continue;
          }

          propertyType = composite.propertyTypeByPropertyName.get(key);
          next = this[propertyType.objectType.singularName](value);
        }

        if (propertyType || include || next) {
          return {
            propertyType,
            next,
            include,
          } as Select;
        }

        return undefined;
      };
    }
  }
}
