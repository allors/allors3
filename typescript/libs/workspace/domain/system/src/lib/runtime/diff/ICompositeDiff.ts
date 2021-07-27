import { IDiff } from "./IDiff";

export interface ICompositeDiff extends IDiff {
    
  OriginalRoleId : number;
  
  ChangedRoleId : number;
}