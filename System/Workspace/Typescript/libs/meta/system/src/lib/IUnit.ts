import { IObjectType } from "./IObjectType";

export interface IUnit extends IObjectType {
  IsBinary: boolean;

  IsBoolean: boolean;

  IsDecimal: boolean;

  IsDateTime: boolean;

  IsFloat: boolean;

  IsInteger: boolean;

  IsString: boolean;

  IsUnique: boolean;

  UnitTag: UnitTags;
}
