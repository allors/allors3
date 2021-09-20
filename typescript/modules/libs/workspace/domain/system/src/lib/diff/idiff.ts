import { RelationType } from "@allors/workspace/meta/system";
import { IStrategy } from "../IStrategy";

export interface IDiff {
    
  relationType : RelationType;
  
  assocation : IStrategy;
}
