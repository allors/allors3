import { Result } from '@allors/workspace/domain/system';
import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazySelectBuilder } from './LazySelectBuilder';
import { LazyTreeBuilder } from './LazyTreeBuilder';

export class LazyResultBuilder {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (obj) => {
        const result: Result = {
          selectRef: obj.selectRef,
          name: obj.name,
          skip: obj.skip,
          take: obj.take,
        };

        if (obj.select) {
          const selectBuilder = metaPopulation['selectBuilder'] as LazySelectBuilder;
          result.select = selectBuilder[composite.singularName](obj.select);
        }

        if (obj.include) {
          if (obj.select) {
            throw new Error('Can not add include when result already has a select.');
          }

          const treeBuilder = metaPopulation['treeBuilder'] as LazyTreeBuilder;
          result.include = treeBuilder[composite.singularName](obj.include);
        }

        return result;
      };
    }
  }
}
