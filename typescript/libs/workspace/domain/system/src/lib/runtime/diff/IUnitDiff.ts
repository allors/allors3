import { IUnit } from "../Types";
import { IDiff } from "./IDiff";

export interface IUnitDiff extends IDiff {
    
  originalRole : IUnit;
  
  changedRole : IUnit;
}
