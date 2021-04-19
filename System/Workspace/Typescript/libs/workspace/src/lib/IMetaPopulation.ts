import { IClass } from './IClass';
import { IComposite } from './IComposite';
import { IInterface } from './IInterface';
import { IMetaObject } from './IMetaObject';
import { IMethodType } from './IMethodType';
import { IUnit } from './IUnit';

export interface IMetaPopulation {
  Units: IUnit[];

  Interfaces: IInterface[];

  Classes: IClass[];

  RelationTypes: IRelationType[];

  MethodTypes: IMethodType[];

  Composites: IComposite[];

  Find(metaObjectId: string): IMetaObject;

  FindByName(name: string): IComposite;
}
