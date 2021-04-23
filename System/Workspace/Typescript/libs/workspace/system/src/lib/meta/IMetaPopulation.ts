import { IClass } from './IClass';
import { IInterface } from './IInterface';
import { IMetaObject } from './IMetaObject';
import { IMethodType } from './IMethodType';
import { IUnit } from './IUnit';
import { IRelationType } from './IRelationType';
import { IComposite } from './IComposite';

export interface IMetaPopulation {
  metaObjectByTag: IMetaObject[];
  units: IUnit[];
  interfaces: IInterface[];
  classes: IClass[];
  composites: IComposite[];
  relationTypes: IRelationType[];
  methodTypes: IMethodType[];
}
