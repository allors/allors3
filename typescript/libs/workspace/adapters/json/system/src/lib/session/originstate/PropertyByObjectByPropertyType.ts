import { PropertyType } from "@allors/workspace/meta/system";
import { MapMap } from "../../collections/MapMap";

export class PropertyByObjectByPropertyType {

  private propertyByObjectByPropertyType: MapMap<PropertyType, number, any>;

  private changedPropertyByObjectByPropertyType: MapMap<PropertyType, number, any>;

  public constructor () {
      this.propertyByObjectByPropertyType = new MapMap();
      this.changedPropertyByObjectByPropertyType = new MapMap();
  }

  public Get(object: number, propertyType: PropertyType): any {
      if (this.changedPropertyByObjectByPropertyType.has(propertyType,object)) {
          return this.changedPropertyByObjectByPropertyType.get(propertyType,object);
      }

      return this.propertyByObjectByPropertyType.get(propertyType, object);
  }

  public Set(object: number, propertyType: PropertyType, newValue: Object) {
      if (!(this.propertyByObjectByPropertyType.TryGetValue(propertyType, /* out */var, valueByPropertyType) && valueByPropertyType.TryGetValue(object, /* out */var, originalValue))) {
          originalValue = null;
      }

      this.changedPropertyByObjectByPropertyType.TryGetValue(propertyType, /* out */var, changedValueByPropertyType);
      if (Equals(newValue, originalValue)) {
          changedValueByPropertyType?.Remove(object);
          // TODO: Warning!!!, inline IF is not supported ?
          propertyType.IsOne;
          this.numbers.AreEqual(newValue, originalValue);
      }
      else {
          if ((changedValueByPropertyType == null)) {
              changedValueByPropertyType = new Dictionary<number, Object>();
              this.changedPropertyByObjectByPropertyType.Add(propertyType, changedValueByPropertyType);
          }

          changedValueByPropertyType[object] = newValue;
      }

  }

  public Checkpoint(): IDictionary<IPropertyType, IDictionary<number, Object>> {
      try {
          let changesSet = this.changedPropertyByObjectByPropertyType;
          for (let kvp in changesSet) {
              let propertyType = kvp.Key;
              let changedPropertyByObject = kvp.Value;
              this.propertyByObjectByPropertyType.TryGetValue(propertyType, /* out */var, propertyByObject);
              for (let kvp2 in changedPropertyByObject) {
                  let object = kvp2.Key;
                  let changedProperty = kvp2.Value;
                  if ((changedProperty == null)) {
                      propertyByObject?.Remove(object);
                  }
                  else {
                      if ((propertyByObject == null)) {
                          propertyByObject = new Dictionary<number, Object>();
                          this.propertyByObjectByPropertyType.Add(propertyType, propertyByObject);
                      }

                      propertyByObject[object] = changedProperty;
                  }

              }

              if ((propertyByObject?.Count == 0)) {
                  this.propertyByObjectByPropertyType.Remove(propertyType);
              }

          }

          return changesSet;
      }
      finally {
          this.changedPropertyByObjectByPropertyType = new Dictionary<IPropertyType, IDictionary<number, Object>>();
      }

  }
}
