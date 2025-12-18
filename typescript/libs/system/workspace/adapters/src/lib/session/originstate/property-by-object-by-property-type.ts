import { IObject } from '@allors/system/workspace/domain';
import { PropertyType } from '@allors/system/workspace/meta';
import { MapMap } from '../../collections/map-map';
import { IRange, Ranges } from '../../collections/ranges/ranges';

export class PropertyByObjectByPropertyType {
  private propertyByObjectByPropertyType: MapMap<
    PropertyType,
    IObject,
    unknown
  >;

  private changedPropertyByObjectByPropertyType: MapMap<
    PropertyType,
    IObject,
    unknown
  >;

  public constructor(private ranges: Ranges<IObject>) {
    this.propertyByObjectByPropertyType = new MapMap();
    this.changedPropertyByObjectByPropertyType = new MapMap();
  }

  public get(object: IObject, propertyType: PropertyType): unknown {
    if (this.changedPropertyByObjectByPropertyType.has(propertyType, object)) {
      return this.changedPropertyByObjectByPropertyType.get(
        propertyType,
        object
      );
    }

    return this.propertyByObjectByPropertyType.get(propertyType, object);
  }

  public set(object: IObject, propertyType: PropertyType, newValue: unknown) {
    const originalValue = this.propertyByObjectByPropertyType.get(
      propertyType,
      object
    ) as IRange<IObject>;

    if (
      propertyType.isOne
        ? newValue === originalValue
        : this.ranges.equals(newValue as IObject[], originalValue)
    ) {
      this.changedPropertyByObjectByPropertyType.remove(propertyType, object);
    } else {
      this.changedPropertyByObjectByPropertyType.set(
        propertyType,
        object,
        newValue
      );
    }
  }

  public checkpoint(): MapMap<PropertyType, IObject, unknown> {
    try {
      const changeSet = this.changedPropertyByObjectByPropertyType;

      changeSet.mapMap.forEach((changedMap, propertyType) => {
        let propertyByObject =
          this.propertyByObjectByPropertyType.mapMap.get(propertyType);

        changedMap.forEach((changedProperty, object) => {
          if (changedProperty == null) {
            propertyByObject?.delete(object);
          } else {
            if (propertyByObject == null) {
              propertyByObject = new Map();
              this.propertyByObjectByPropertyType.mapMap.set(
                propertyType,
                propertyByObject
              );
            }

            propertyByObject.set(object, changedProperty);
          }
        });

        if (propertyByObject?.size === 0) {
          this.propertyByObjectByPropertyType.mapMap.delete(propertyType);
        }
      });

      return changeSet;
    } finally {
      this.changedPropertyByObjectByPropertyType = new MapMap();
    }
  }
}
