import { Class } from './class';
import { Interface } from './interface';
import { MetaObject } from './meta-object';
import { MethodType } from './method-type';
import { Unit } from './unit';
import { RelationType } from './relation-type';
import { Composite } from './composite';
import { PropertyType } from './property-type';
import { Dependency } from './dependency';

export interface MetaPopulation {
  readonly kind: 'MetaPopulation';
  metaObjectByTag: Map<string, MetaObject>;
  objectTypeByUppercaseName: Map<string, MetaObject>;
  units: Set<Unit>;
  interfaces: Set<Interface>;
  classes: Set<Class>;
  composites: Set<Composite>;
  relationTypes: Set<RelationType>;
  methodTypes: Set<MethodType>;

  dependency: <T extends Composite>(
    objectType: T,
    propertyType: (objectType: T) => PropertyType
  ) => Dependency;
}
