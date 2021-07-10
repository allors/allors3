import { IObject } from './IObject';

export type UnitType = string | Date | boolean | number;

export type TypeForRole = UnitType | IObject | IObject[];

export type TypeForAssociation = IObject | IObject[];

export type TypeForParameter = UnitType | IObject | IObject[];

// todo: move to Database
export function isSessionObject(obj: unknown): obj is IObject {
  return (obj as IObject).id != null;
}
