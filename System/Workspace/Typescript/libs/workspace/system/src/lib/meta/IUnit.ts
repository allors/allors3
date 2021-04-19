import { UnitTags } from '../UnitTags';
import { IObjectType } from './IObjectType';

export interface IUnit extends IObjectType {
  isBinary: boolean;

  isBoolean: boolean;

  isDecimal: boolean;

  isDateTime: boolean;

  isFloat: boolean;

  isInteger: boolean;

  isString: boolean;

  isUnique: boolean;

  unitTag: UnitTags;
}
