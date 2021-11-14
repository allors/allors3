import { Origin } from './origin';
import { ObjectType } from './object-type';
import { OperandType } from './operand-type';

export interface PropertyType extends OperandType {
  _: unknown;
  isRoleType: boolean;
  isAssociationType: boolean;
  isMethodType: boolean;
  origin: Origin;
  singularName: string;
  pluralName: string;
  objectType: ObjectType;
  isOne: boolean;
  isMany: boolean;
}
