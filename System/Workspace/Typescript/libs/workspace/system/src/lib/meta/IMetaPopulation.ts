import { IClass } from './IClass';
import { IInterface } from './IInterface';
import { IMetaObject } from './IMetaObject';
import { IMethodType } from './IMethodType';
import { IUnit } from './IUnit';
import { IRelationType } from './IRelationType';

export interface IMetaPopulation {
  metaObjectByTag: IMetaObject[];
  units: IUnit[];
  interfaces: IInterface[];
  classes: IClass[];
  relationTypes: IRelationType[];
  methodTypes: IMethodType[];
}
