import { Select } from '@allors/system/workspace/domain';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyTreeBuilder } from './lazy-tree-builder';

export class LazySelectBuilder {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (obj, previous: Select) => {
        if (obj['propertyType']) {
          return obj;
        }

        const current: Partial<Select> = {};
        for (const [key, value] of Object.entries(obj)) {
          switch (key.valueOf()) {
            case 'include':
              if (previous) {
                previous.include = (
                  metaPopulation['treeBuilder'] as LazyTreeBuilder
                )[composite.singularName](value);
              } else {
                current.include = (
                  metaPopulation['treeBuilder'] as LazyTreeBuilder
                )[composite.singularName](value);
              }
              break;
            default:
              current.propertyType =
                composite.propertyTypeByPropertyName.get(key);
              current.next = this[current.propertyType.objectType.singularName](
                value,
                current
              );
              break;
          }
        }

        return current.propertyType || current.include ? current : undefined;
      };
    }
  }
}
