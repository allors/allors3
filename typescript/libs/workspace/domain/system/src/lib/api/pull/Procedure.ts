import { IObject } from '../../IObject';

export interface Procedure {
  name: string;

  collections?: { [name: string]: IObject[] } | Map<string, IObject[]>;

  objects?: { [name: string]: IObject } | Map<string, IObject>;

  values?: { [name: string]: string } | Map<string, string>;

  pool?: Map<IObject, number>;
}
