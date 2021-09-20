import { IStrategy } from "../IStrategy";
import { IDiff } from "./IDiff";

export interface ICompositesDiff extends IDiff {
    
  originalRoles : Readonly<IStrategy[]>;
  
  changedRoles : Readonly<IStrategy[]>;
}
