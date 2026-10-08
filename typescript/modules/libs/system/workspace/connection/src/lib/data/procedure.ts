import { IIdentifiable } from '../types';

export interface Procedure {
  name: string;

  collections?: { [name: string]: IIdentifiable[] } | Map<string, IIdentifiable[]>;

  objects?: { [name: string]: IIdentifiable } | Map<string, IIdentifiable>;

  values?: { [name: string]: string } | Map<string, string>;

  pool?: Map<IIdentifiable, number>;
}
