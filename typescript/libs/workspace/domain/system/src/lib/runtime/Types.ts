import { IObject } from './IObject';

export type IUnit = string | Date | boolean | number;

export type TypeForRole = IUnit | IObject | IObject[];

export type TypeForAssociation = IObject | IObject[];

export type TypeForParameter = IUnit | IObject | IObject[];

// todo: move to Database
export function isSessionObject(obj: unknown): obj is IObject {
  return (obj as IObject).id != null;
}
