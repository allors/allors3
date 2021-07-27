import { IDiff } from "./IDiff";

export interface ICompositesDiff extends IDiff {
    
  OriginalRoleIds : number[];
  
  ChangedRoleIds : number[];
}
