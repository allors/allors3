import { Class } from './Class';
import { Interface } from './Interface';
import { MetaObject } from './MetaObject';
import { MethodType } from './MethodType';
import { Unit } from './Unit';
import { RelationType } from './RelationType';
import { Composite } from './Composite';

export interface MetaPopulation {
  metaObjectByTag: Readonly<Map<number, MetaObject>>;
  units: Readonly<Set<Unit>>;
  interfaces: Readonly<Set<Interface>>;
  classes: Readonly<Set<Class>>;
  composites: Readonly<Set<Composite>>;
  relationTypes: Readonly<Set<RelationType>>;
  methodTypes: Readonly<Set<MethodType>>;
}
