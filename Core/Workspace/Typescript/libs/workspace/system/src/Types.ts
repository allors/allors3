import { SessionObject } from './SessionObject';

export type UnitTypes = string | Date | boolean | number;
export type CompositeTypes = SessionObject | string;
export type ParameterTypes = UnitTypes | CompositeTypes | CompositeTypes[];

// todo: move to Database
export function isSessionObject(obj): obj is SessionObject {
  return (obj as SessionObject).id != null;
}
