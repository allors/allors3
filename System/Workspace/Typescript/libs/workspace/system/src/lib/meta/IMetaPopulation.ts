import { IClass } from './IClass';
import { IInterface } from './IInterface';
import { IMetaObject } from './IMetaObject';
import { IMethodType } from './IMethodType';
import { IUnit } from './IUnit';
import { IRelationType } from './IRelationType';
import { IComposite } from './IComposite';

export interface IMetaPopulation {
  metaObjectByTag: Readonly<Map<number, IMetaObject>>;
  units: Readonly<Set<IUnit>>;
  interfaces: Readonly<Set<IInterface>>;
  classes: Readonly<Set<IClass>>;
  composites: Readonly<Set<IComposite>>;
  relationTypes: Readonly<Set<IRelationType>>;
  methodTypes: Readonly<Set<IMethodType>>;
}
