import { PropertyType } from '@allors/workspace/meta/system';
import { MapMap } from '../../collections/MapMap';
import { IRange, Ranges } from '../../collections/ranges/Ranges';
import { Strategy } from '../Strategy';

export class PropertyByObjectByPropertyType {
  private propertyByObjectByPropertyType: MapMap<PropertyType, Strategy, unknown>;

  private changedPropertyByObjectByPropertyType: MapMap<PropertyType, Strategy, unknown>;

  public constructor(private ranges: Ranges<Strategy>) {
    this.propertyByObjectByPropertyType = new MapMap();
    this.changedPropertyByObjectByPropertyType = new MapMap();
  }

  public get(object: Strategy, propertyType: PropertyType): unknown {
    if (this.changedPropertyByObjectByPropertyType.has(propertyType, object)) {
      return this.changedPropertyByObjectByPropertyType.get(propertyType, object);
    }

    return this.propertyByObjectByPropertyType.get(propertyType, object);
  }

  public set(object: Strategy, propertyType: PropertyType, newValue: unknown) {
    const originalValue = this.propertyByObjectByPropertyType.get(propertyType, object) as IRange<Strategy>;

    if (propertyType.isOne ? newValue === originalValue : this.ranges.equals(newValue as Strategy[], originalValue)) {
      this.changedPropertyByObjectByPropertyType.remove(propertyType, object);
    } else {
      this.changedPropertyByObjectByPropertyType.set(propertyType, object, newValue);
    }
  }

  public checkpoint(): MapMap<PropertyType, Strategy, unknown> {
    try {
      const changeSet = this.changedPropertyByObjectByPropertyType;

      changeSet.mapMap.forEach((changedMap, propertyType) => {
        let propertyByObject = this.propertyByObjectByPropertyType.mapMap.get(propertyType);

        changedMap.forEach((changedProperty, object) => {
          if (changedProperty == null) {
            propertyByObject?.delete(object);
          } else {
            if (propertyByObject == null) {
              propertyByObject = new Map();
              this.propertyByObjectByPropertyType.mapMap.set(propertyType, propertyByObject);
            }

            propertyByObject.set(object, changedProperty);
          }
        });

        if (propertyByObject.size === 0) {
          this.propertyByObjectByPropertyType.mapMap.delete(propertyType);
        }
      });

      return changeSet;
    } finally {
      this.changedPropertyByObjectByPropertyType = new MapMap();
    }
  }
}
