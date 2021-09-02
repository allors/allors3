import { MetaPopulation } from '@allors/workspace/meta/system';
import { Filter, FlatPull, Pull, Result, Node } from '@allors/workspace/domain/system';
import { LazySelections } from './LazySelections';
import { LazyTrees } from './LazyTrees';

export class LazyPulls {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (flat: FlatPull) => {
        const pull: Pull = {
          extentRef: flat.extentRef,
          extent: flat.extent,
          object: flat.object,
          results: flat.results,
          arguments: flat.arguments,
        };

        if (flat.predicate) {
          if (pull.object || pull.extent || pull.extentRef) {
            throw new Error('predicate conflicts with object/extent/extentRef');
          }

          const filter: Filter = {
            kind: 'Filter',
            objectType: pull.objectType,
            predicate: flat.predicate,
            sorting: flat.sorting,
          };

          pull.extent = filter;
        }

        if (!pull.object && !pull.extent && !pull.extentRef) {
          const filter: Filter = {
            kind: 'Filter',
            objectType: composite,
            sorting: flat.sorting,
          };

          pull.extent = filter;
        }

        if (flat.selectRef || flat.select || flat.include || flat.name || flat.skip || flat.take) {
          const result: Result = {
            selectRef: flat.selectRef,
            name: flat.name,
            skip: flat.skip,
            take: flat.take,
          };

          if (flat.select) {
            if (flat.select.propertyType || flat.select.include || flat.select.next) {
              result.select = flat.select;
            } else {
              const selections = metaPopulation['selections'] as LazySelections;
              result.select = selections[composite.singularName](flat.select);
            }
          }

          if (flat.include) {
            if (Array.isArray(flat.include)) {
              result.select = {
                include: flat.include as Node[],
              };
            } else {
              const trees = metaPopulation['trees'] as LazyTrees;
              result.select = {
                include: trees[composite.singularName](flat.include),
              };
            }
          }

          pull.results = pull.results || [];
          pull.results.push(result);
        }

        return undefined;
      };
    }
  }
}
