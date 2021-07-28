import { IDiff } from "./IDiff";

export interface ICompositesDiff extends IDiff {
    
  originalRoleIds : number[];
  
  changedRoleIds : number[];
}
