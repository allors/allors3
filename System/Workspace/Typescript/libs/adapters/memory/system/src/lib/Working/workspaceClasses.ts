import { ObjectType } from '@allors/meta/core';
import { DatabaseObject } from '@allors/workspace/system/core';
import { MemoryWorkspaceObject } from './WorkspaceObject';

export function workspaceClasses(classes: ObjectType[], constructorByObjectType: Map<ObjectType, any>) {
  classes.forEach((objectType) => {
    const DynamicClass = (() => {
      return function () {
        const prototype1 = Object.getPrototypeOf(this);
        const prototype2 = Object.getPrototypeOf(prototype1);
        prototype2.init.call(this);
      };
    })();

    DynamicClass.prototype = Object.create(MemoryWorkspaceObject.prototype);
    DynamicClass.prototype.constructor = DynamicClass;
    constructorByObjectType.set(objectType, DynamicClass);

    const prototype = DynamicClass.prototype;
    objectType.roleTypes.forEach((roleType) => {
      Object.defineProperty(prototype, roleType.name, {
        get(this: DatabaseObject) {
          return this.get(roleType);
        },

        set(this: DatabaseObject, value) {
          this.set(roleType, value);
        },
      });

      if (roleType.isMany) {
        prototype['Add' + roleType.singular] = function (this: DatabaseObject, value: DatabaseObject) {
          return this.add(roleType, value);
        };

        prototype['Remove' + roleType.singular] = function (this: DatabaseObject, value: DatabaseObject) {
          return this.remove(roleType, value);
        };
      }
    });

    objectType.associationTypes.forEach((associationType) => {
      Object.defineProperty(prototype, associationType.name, {
        get(this: DatabaseObject) {
          return this.getAssociation(associationType);
        },
      });
    });
  });
}
