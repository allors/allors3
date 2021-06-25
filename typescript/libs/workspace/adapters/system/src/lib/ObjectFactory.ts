import { IObject, IObjectFactory, IStrategy } from '@allors/workspace/domain/system';
import { MetaPopulation, ObjectType } from '@allors/workspace/meta/system';
import { ObjectBase } from './ObjectBase';

export class ObjectFactory implements IObjectFactory {
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
      objectType.roleTypes.forEach((roleType) => {
        Object.defineProperty(prototype, 'CanRead' + roleType.name, {
          get(this: ObjectBase) {
            return this.strategy.canRead(roleType);
          },
        });

        if (roleType.relationType.isDerived) {
          Object.defineProperty(prototype, roleType.name, {
            get(this: ObjectBase) {
              return this.strategy.get(roleType);
            },
          });
        } else {
          Object.defineProperty(prototype, 'CanWrite' + roleType.name, {
            get(this: ObjectBase) {
              return this.strategy.canWrite(roleType);
            },
          });

          Object.defineProperty(prototype, roleType.name, {
            get(this: ObjectBase) {
              return this.strategy.get(roleType);
            },

            set(this: ObjectBase, value) {
              this.strategy.set(roleType, value);
            },
          });

          if (roleType.isMany) {
            prototype['Add' + roleType.singularName] = function (this: ObjectBase, value: ObjectBase) {
              return this.strategy.add(roleType, value);
            };

            prototype['Remove' + roleType.singularName] = function (this: ObjectBase, value: ObjectBase) {
              return this.strategy.remove(roleType, value);
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
        Object.defineProperty(prototype, 'CanExecute' + methodType.name, {
          get(this: ObjectBase) {
            return this.strategy.canExecute(methodType);
          },
        });

        Object.defineProperty(prototype, methodType.name, {
          get(this: ObjectBase) {
            return this.strategy.method(methodType);
          },
        });
      });
    });
  }

  create(strategy: IStrategy): IObject {
    const constructor = this.constructorByObjectType.get(strategy.cls);
    if (!constructor) {
      throw new Error(`Could not get constructor for ${strategy.cls.singularName}`);
    }

    const newObject: ObjectBase = new constructor();
    newObject.strategy = strategy;

    return newObject;
  }
}
