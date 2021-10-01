import { MetaPopulation } from '@allors/workspace/meta/system';

export class LazyTreeBuilder {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      this[composite.singularName] = (obj) => {
        if(Array.isArray(obj)){
          return obj;
        }
        
        const entries = Object.entries(obj);
        return entries.length > 0
          ? entries.map(([key, value]) => {
              const propertyType = composite.propertyTypeByPropertyName.get(key);
              try {
                return value != null
                  ? {
                      propertyType,
                      nodes: this[propertyType.objectType.singularName](value),
                    }
                  : {
                      propertyType,
                    };
              } catch (e) {
                console.error(e);
                return null;
              }
            })
          : undefined;
      };
    }
  }
}
