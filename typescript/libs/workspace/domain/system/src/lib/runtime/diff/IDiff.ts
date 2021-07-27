import { RelationType } from "@allors/workspace/meta/system";
import { IStrategy } from "../IStrategy";

export interface IDiff {
    
  RelationType : RelationType;
  
  Assocation : IStrategy;
}
