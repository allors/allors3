import { DatabaseObject } from './Session/DatabaseObject';

export type UnitTypes = string | Date | boolean | number;
export type CompositeTypes = DatabaseObject | string;
export type ParameterTypes = UnitTypes | CompositeTypes | CompositeTypes[];

// todo: move to Database
export function isSessionObject(obj): obj is DatabaseObject {
  return (obj as DatabaseObject).id != null;
}
