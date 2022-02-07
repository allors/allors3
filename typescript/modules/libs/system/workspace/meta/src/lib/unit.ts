import { ObjectType } from './object-type';

export interface Unit extends ObjectType {
  readonly kind: 'Unit';
  isBinary: boolean;
  isBoolean: boolean;
  isDecimal: boolean;
  isDateTime: boolean;
  isFloat: boolean;
  isInteger: boolean;
  isString: boolean;
  isUnique: boolean;
}
