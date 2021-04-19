import { IClass } from './IClass';
import { IComposite } from './IComposite';
import { IInterface } from './IInterface';
import { IMetaObject } from './IMetaObject';
import { IMethodType } from './IMethodType';
import { IUnit } from './IUnit';
import { IRelationType } from './IRelationType';

export interface IMetaPopulation {
  units: IUnit[];

  interfaces: IInterface[];

  classes: IClass[];

  relationTypes: IRelationType[];

  methodTypes: IMethodType[];

  composites: IComposite[];

  find(metaObjectId: string): IMetaObject;

  findByName(name: string): IComposite;
}
