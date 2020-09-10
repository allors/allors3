import { Composite } from './Composite';

export type UnitTypes = string | Date | boolean | number;
export type CompositeTypes = Composite | string;
export type ParameterTypes = UnitTypes | CompositeTypes | CompositeTypes[];

// todo: move to Database
export function isSessionObject(obj): obj is Composite {
  return (obj as Composite).id != null;
}
