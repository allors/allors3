import { Strategy } from "@allors/workspace/adapters/system";
import { IDiff } from "./IDiff";

export interface ICompositesDiff extends IDiff {
    
  originalRoles : Readonly<Strategy[]>;
  
  changedRoles : Readonly<Strategy[]>;
}
