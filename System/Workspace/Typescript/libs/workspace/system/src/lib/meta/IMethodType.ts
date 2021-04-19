import { IComposite } from './IComposite';
import { IMetaObject } from './IMetaObject';
import { IOperandType } from './IOperandType';

export interface IMethodType extends IMetaObject, IOperandType {
  objectType: IComposite;
}
