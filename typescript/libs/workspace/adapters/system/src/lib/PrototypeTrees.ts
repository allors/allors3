import { Node } from '@allors/workspace/domain/system';
import { MetaPopulation, PropertyType, Interface } from '@allors/workspace/meta/system';

export class PrototypeTrees {
  constructor(metaPopulation: MetaPopulation) {
    for (const composite of metaPopulation.composites) {
      const propertyTypeByPropertyName: Map<string, PropertyType> = new Map();
      for (const roleType of composite.roleTypes) {
        propertyTypeByPropertyName.set(roleType.name, roleType);
      }

      for (const associationType of composite.associationTypes) {
        propertyTypeByPropertyName.set(associationType.name, associationType);
      }

      if (composite.isInterface) {
        for (const subtype of (composite as Interface).subtypes) {
          for (const roleType of subtype.roleTypes) {
            propertyTypeByPropertyName.set(subtype.singularName + '_' + roleType.name, roleType);
          }

          for (const associationType of subtype.associationTypes) {
            propertyTypeByPropertyName.set(subtype.singularName + '_' + associationType.name, associationType);
          }
        }
      }

      this[composite.singularName] = (obj) => {
        return Object.entries(obj).map(([key, value]) => {
          const propertyType = propertyTypeByPropertyName.get(key);
          const nodes = value != null ? this[propertyType.objectType.singularName](value) : [];
          return {
            propertyType,
            nodes,
          };
        });
      };
    }
  }
}
