import { Class } from './class';
import { Interface } from './interface';
import { MetaObject } from './meta-object';
import { MethodType } from './method-type';
import { Unit } from './unit';
import { RelationType } from './relation-type';
import { Composite } from './composite';

export interface MetaPopulation {
  readonly kind: 'MetaPopulation';
  metaObjectByTag: Map<string, MetaObject>;
  units: Set<Unit>;
  interfaces: Set<Interface>;
  classes: Set<Class>;
  composites: Set<Composite>;
  relationTypes: Set<RelationType>;
  methodTypes: Set<MethodType>;
}
