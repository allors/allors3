import { IObject } from "../../runtime/IObject";

export interface Procedure {
  name: string;

  collections?: Map<string, IObject[]>;

  objects?: Map<string, IObject>;

  values?: Map<string, string>;

  pool?: Map<IObject, number>;
}
