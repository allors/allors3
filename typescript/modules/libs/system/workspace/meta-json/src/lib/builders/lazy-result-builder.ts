import { Result } from '@allors/system/workspace/domain';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazySelectBuilder } from './lazy-select-builder';
import { LazyTreeBuilder } from './lazy-tree-builder';

export class LazyResultBuilder {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (object) => {
        const result: Result = {
          selectRef: object.selectRef,
          name: object.name,
          skip: object.skip,
          take: object.take,
        };

        if (object.select) {
          const selectBuilder = metaPopulation[
            'selectBuilder'
          ] as LazySelectBuilder;
          result.select = selectBuilder[composite.singularName](object.select);
        }

        if (object.include) {
          if (object.select) {
            throw new Error(
              'Can not add include when result already has a select.'
            );
          }

          const treeBuilder = metaPopulation['treeBuilder'] as LazyTreeBuilder;
          result.include = treeBuilder[composite.singularName](object.include);
        }

        return result;
      };
    }
  }
}
