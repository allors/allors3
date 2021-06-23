import { PropertyType } from '@allors/workspace/system';
import { MapMap } from '../../collections/MapMap';
import { equals, Numbers } from '../../collections/Numbers';

export class PropertyByObjectByPropertyType {
  private propertyByObjectByPropertyType: MapMap<PropertyType, number, any>;

  private changedPropertyByObjectByPropertyType: MapMap<PropertyType, number, any>;

  public constructor() {
    this.propertyByObjectByPropertyType = new MapMap();
    this.changedPropertyByObjectByPropertyType = new MapMap();
  }

  public Get(object: number, propertyType: PropertyType): any {
    if (this.changedPropertyByObjectByPropertyType.has(propertyType, object)) {
      return this.changedPropertyByObjectByPropertyType.get(propertyType, object);
    }

    return this.propertyByObjectByPropertyType.get(propertyType, object);
  }

  public Set(object: number, propertyType: PropertyType, newValue: any) {
    const originalValue = this.propertyByObjectByPropertyType.get(propertyType, object) as Numbers;

    if (propertyType.isOne ? newValue === originalValue : equals(newValue, originalValue)) {
      this.changedPropertyByObjectByPropertyType.remove(propertyType, object);
    } else {
      this.changedPropertyByObjectByPropertyType.set(propertyType, object, newValue);
    }
  }

  public Checkpoint(): MapMap<PropertyType, number, any> {
    try {
      const changeSet = this.changedPropertyByObjectByPropertyType;
      const original = this.propertyByObjectByPropertyType;

      changeSet.mapMap.forEach((map, propertyType) => {
        let originalMap = original.mapMap.get(propertyType);

        map.forEach((value, object) => {
          if (value == null) {
            originalMap?.delete(object);
          } else {
            if (originalMap == null) {
              originalMap = new Map();
              original.mapMap.set(propertyType, originalMap);
            }

            originalMap.set(object, value);
          }
        });

        if (originalMap.size === 0) {
          original.mapMap.delete(propertyType);
        }
      });

      return changeSet;
    } finally {
      this.changedPropertyByObjectByPropertyType = new MapMap();
    }
  }
}
