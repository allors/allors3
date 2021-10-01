import { Origin } from './origin';
import { ObjectType } from './object-type';
import { OperandType, OperandTypeExtension } from './operand-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface PropertyTypeExtension extends OperandTypeExtension {}

export interface PropertyType extends OperandType {
  _: PropertyTypeExtension;
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
