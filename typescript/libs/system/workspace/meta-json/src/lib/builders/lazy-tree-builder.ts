import { MetaPopulation } from '@allors/system/workspace/meta';

export class LazyTreeBuilder {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (obj) => {
        if (Array.isArray(obj)) {
          return obj;
        }

        const entries = Object.entries(obj);
        return entries.length > 0
          ? entries.map(([key, value]) => {
              const propertyType =
                composite.propertyTypeByPropertyName.get(key);
              return value != null
                ? {
                    propertyType,
                    ofType: this['ofType'],
                    nodes: this[propertyType.objectType.singularName](value),
                  }
                : {
                    propertyType,
                    ofType: this['ofType'],
                  };
            })
          : undefined;
      };
    }
  }
}
