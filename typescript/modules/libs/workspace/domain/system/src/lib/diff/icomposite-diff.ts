import { IStrategy } from "../istrategy";
import { IDiff } from "./idiff";

export interface ICompositeDiff extends IDiff {
    
  originalRole : IStrategy;
  
  changedRole : IStrategy;
}