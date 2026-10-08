import { MetaObject } from '@allors/system/workspace/meta';

/**
 * Anything the workspace knows by an id: an object of a session, or a bare id in a layer
 * without objects. The connection reads the id and nothing else. A database object has a
 * positive id; a new object that has not been pushed has a negative one.
 */
export interface IIdentifiable {
  id: number;
}

export type IUnit = string | Date | boolean | number;

export type TypeForParameter =
  | IUnit
  | IIdentifiable
  | IIdentifiable[]
  | MetaObject;
