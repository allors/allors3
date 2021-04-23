import { Origin } from '../Origin';
import { IObjectType } from './IObjectType';
import { IOperandType } from './IOperandType';

  export interface IPropertyType extends IOperandType {
  origin: Origin;

  singularName: string;

  pluralName: string;

  objectType: IObjectType;

  isOne: boolean;

  isMany: boolean;

  // get(strategy: IStrategy, ofType: IComposite): object;
}
