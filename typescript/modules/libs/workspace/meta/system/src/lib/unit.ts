import { ObjectType, ObjectTypeExtension } from './object-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface UnitExtension extends ObjectTypeExtension {}

export interface Unit extends ObjectType {
  readonly kind: 'Unit';
  _: UnitExtension;
  isBinary: boolean;
  isBoolean: boolean;
  isDecimal: boolean;
  isDateTime: boolean;
  isFloat: boolean;
  isInteger: boolean;
  isString: boolean;
  isUnique: boolean;
}
