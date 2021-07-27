import { IUnit } from "../Types";
import { IDiff } from "./IDiff";

export interface IUnitDiff extends IDiff {
    
  OriginalRole : IUnit;
  
  ChangedRole : IUnit;
}
