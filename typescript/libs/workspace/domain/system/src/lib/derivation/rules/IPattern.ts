import { Composite } from "@allors/workspace/meta/system";
import { IAssociationPattern } from "./IAssociationPattern";
import { IRolePattern } from "./IRolePattern";

export interface IPatternBase {
    
    Tree : Node[];
    
    ofType : Composite;
    
    objectType : Composite;
}

export type IPattern = IRolePattern | IAssociationPattern;

export type IPatternKind = IPattern['kind'];