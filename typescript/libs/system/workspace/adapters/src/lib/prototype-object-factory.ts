import {
  IObject,
  IObjectFactory,
  IStrategy,
} from '@allors/system/workspace/domain';
import {
  MetaPopulation,
  ObjectType,
  Origin,
} from '@allors/system/workspace/meta';

import { ObjectBase } from './object-base';

export class PrototypeObjectFactory implements IObjectFactory {
  constructorByObjectType: Map<ObjectType, any>;

  constructor(public metaPopulation: MetaPopulation) {
    this.constructorByObjectType = new Map();

    this.metaPopulation.classes.forEach((objectType) => {
      const DynamicClass = (() => {
        return function () {
          const prototype1 = Object.getPrototypeOf(this);
          const prototype2 = Object.getPrototypeOf(prototype1);
          prototype2.init.call(this);
        };
      })();

      DynamicClass.prototype = Object.create(ObjectBase.prototype);
      DynamicClass.prototype.constructor = DynamicClass;
      this.constructorByObjectType.set(objectType, DynamicClass as any);

      const prototype = DynamicClass.prototype;

      prototype.toString = function () {
        return this.strategy.toString();
      };

      prototype.toJSON = function () {
        return this.strategy.ToJSON();
      };

      objectType.roleTypes.forEach((roleType) => {
        Object.defineProperty(prototype, 'canRead' + roleType.name, {
          get(this: ObjectBase) {
            return this.strategy.canRead(roleType);
          },
        });

        Object.defineProperty(prototype, 'canWrite' + roleType.name, {
          get(this: ObjectBase) {
            return this.strategy.canWrite(roleType);
          },
        });

        const relationType = roleType.relationType;
        const isDatabase = relationType.origin === Origin.Database;
        const isDerived = relationType.isDerived;

        if (isDatabase && isDerived) {
          Object.defineProperty(prototype, roleType.name, {
            get(this: ObjectBase) {
              return this.strategy.getRole(roleType);
            },
          });
        } else {
          Object.defineProperty(prototype, roleType.name, {
            get(this: ObjectBase) {
              return this.strategy.getRole(roleType);
            },

            set(this: ObjectBase, value) {
              this.strategy.setRole(roleType, value);
            },
          });

          if (roleType.isMany) {
            prototype['add' + roleType.singularName] = function (
              this: ObjectBase,
              value: ObjectBase
            ) {
              return this.strategy.addCompositesRole(roleType, value);
            };

            prototype['remove' + roleType.singularName] = function (
              this: ObjectBase,
              value: ObjectBase
            ) {
              return this.strategy.removeCompositesRole(roleType, value);
            };
          }
        }
      });

      objectType.associationTypes.forEach((associationType) => {
        if (associationType.isOne) {
          Object.defineProperty(prototype, associationType.name, {
            get(this: ObjectBase) {
              return this.strategy.getCompositeAssociation(associationType);
            },
          });
        } else {
          Object.defineProperty(prototype, associationType.name, {
            get(this: ObjectBase) {
              return this.strategy.getCompositesAssociation(associationType);
            },
          });
        }
      });

      objectType.methodTypes.forEach((methodType) => {
        Object.defineProperty(prototype, 'canExecute' + methodType.name, {
          get(this: ObjectBase) {
            return this.strategy.canExecute(methodType);
          },
        });

        Object.defineProperty(prototype, methodType.name, {
          get(this: ObjectBase) {
            return { object: this, methodType };
          },
        });
      });
    });
  }

  create(strategy: IStrategy): IObject {
    const constructor = this.constructorByObjectType.get(strategy.cls);
    if (!constructor) {
      throw new Error(
        `Could not get constructor for ${strategy.cls.singularName}`
      );
    }

    const newObject: ObjectBase = new constructor();
    newObject.strategy = strategy;

    return newObject;
  }
}
