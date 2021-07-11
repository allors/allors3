import { PropertyType } from '@allors/workspace/meta/system';
import { MapMap } from '../../collections/MapMap';
import { equals, IRange } from '../../collections/Range';

export class PropertyByObjectByPropertyType {
  private propertyByObjectByPropertyType: MapMap<PropertyType, number, unknown>;

  private changedPropertyByObjectByPropertyType: MapMap<PropertyType, number, unknown>;

  public constructor() {
    this.propertyByObjectByPropertyType = new MapMap();
    this.changedPropertyByObjectByPropertyType = new MapMap();
  }

  public get(object: number, propertyType: PropertyType): unknown {
    if (this.changedPropertyByObjectByPropertyType.has(propertyType, object)) {
      return this.changedPropertyByObjectByPropertyType.get(propertyType, object);
    }

    return this.propertyByObjectByPropertyType.get(propertyType, object);
  }

  public set(object: number, propertyType: PropertyType, newValue: unknown) {
    const originalValue = this.propertyByObjectByPropertyType.get(propertyType, object) as IRange;

    if (propertyType.isOne ? newValue === originalValue : equals(newValue as number[], originalValue)) {
      this.changedPropertyByObjectByPropertyType.remove(propertyType, object);
    } else {
      this.changedPropertyByObjectByPropertyType.set(propertyType, object, newValue);
    }
  }

  public checkpoint(): MapMap<PropertyType, number, unknown> {
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
