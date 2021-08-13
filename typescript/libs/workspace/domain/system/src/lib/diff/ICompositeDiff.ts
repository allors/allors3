import { IDiff } from "./IDiff";

export interface ICompositeDiff extends IDiff {
    
  originalRoleId : number;
  
  changedRoleId : number;
}