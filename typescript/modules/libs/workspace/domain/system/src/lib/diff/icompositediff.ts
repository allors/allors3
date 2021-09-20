import { IStrategy } from "../IStrategy";
import { IDiff } from "./IDiff";

export interface ICompositeDiff extends IDiff {
    
  originalRole : IStrategy;
  
  changedRole : IStrategy;
}