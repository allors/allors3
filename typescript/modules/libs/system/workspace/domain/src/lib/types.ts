import { IObject } from './iobject';

// The types the query model is written in live in the connection; the session API keeps
// them under its own name.
export { IIdentifiable, IUnit, TypeForParameter } from '@allors/system/workspace/connection';
import { IUnit } from '@allors/system/workspace/connection';

export type TypeForRole = IUnit | IObject | IObject[];

export type TypeForAssociation = IObject | IObject[];

// todo: move to Database
export function isSessionObject(obj: unknown): obj is IObject {
  return (obj as IObject).id != null;
}
