import { MetaPopulation } from '@allors/system/workspace/meta';
import {
  Filter,
  FlatPull,
  Pull,
  Result,
} from '@allors/system/workspace/domain';
import { LazySelectBuilder } from './lazy-select-builder';
import { LazyTreeBuilder } from './lazy-tree-builder';
import { LazyResultBuilder } from './lazy-result-builder';

export class LazyPullBuilder {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (flat: FlatPull) => {
        const pull: Pull = {
          extentRef: flat.extentRef,
          extent: flat.extent,
          results: flat.results,
          arguments: flat.arguments,
        };

        if (flat.object || flat.objectId) {
          pull.objectType = composite;
          pull.object = flat.object;
          if (flat.objectId && !flat.object) {
            if (typeof flat.objectId === 'string') {
              pull.objectId = Number.parseInt(flat.objectId);
            } else {
              pull.objectId = flat.objectId;
            }
          }
        }

        if (flat.predicate) {
          if (pull.object || pull.extent || pull.extentRef) {
            throw new Error('predicate conflicts with object/extent/extentRef');
          }

          const filter: Filter = {
            kind: 'Filter',
            objectType: composite,
            predicate: flat.predicate,
            sorting: flat.sorting,
          };

          pull.extent = filter;
        }

        if (!pull.object && !pull.objectId && !pull.extent && !pull.extentRef) {
          const filter: Filter = {
            kind: 'Filter',
            objectType: composite,
            sorting: flat.sorting,
          };

          pull.extent = filter;
        }

        let results: Result[];

        if (flat.results) {
          results = [];
          const resultBuilder = metaPopulation[
            'resultBuilder'
          ] as LazyResultBuilder;
          for (const flatResult of flat.results) {
            const result = resultBuilder[composite.singularName](flatResult);
            results.push(result);
          }
        }

        if (
          flat.selectRef ||
          flat.select ||
          flat.include ||
          flat.name ||
          flat.skip ||
          flat.take
        ) {
          const result: Result = {
            selectRef: flat.selectRef,
            name: flat.name,
            skip: flat.skip,
            take: flat.take,
          };

          if (flat.select) {
            const selectBuilder = metaPopulation[
              'selectBuilder'
            ] as LazySelectBuilder;
            result.select = selectBuilder[composite.singularName](flat.select);
          }

          if (flat.include) {
            if (flat.select) {
              throw new Error(
                'Can not add include when result already has a select.'
              );
            }

            const treeBuilder = metaPopulation[
              'treeBuilder'
            ] as LazyTreeBuilder;
            result.include = treeBuilder[composite.singularName](flat.include);
          }

          results = results ?? [];
          results.push(result);
        }

        if (results?.length > 0) {
          pull.results = results;
        }

        return pull;
      };
    }
  }
}
