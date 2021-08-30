import { IDiff } from "./IDiff";
import { Strategy } from '@allors/workspace/adapters/system';

export interface ICompositeDiff extends IDiff {
    
  originalRole : Strategy;
  
  changedRole : Strategy;
}