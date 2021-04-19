import { Origin } from '../Origin';
import { IComposite } from './IComposite';
import { IObjectType } from './IObjectType';
import { IOperandType } from './IOperandType';

  // TODO: IComparable
  export interface IPropertyType extends IOperandType {
  origin: Origin;

  name: string;

  singularName: string;

  pluralName: string;

  objectType: IObjectType;

  isOne: boolean;

  isMany: boolean;

  // get(strategy: IStrategy, ofType: IComposite): object;
}
