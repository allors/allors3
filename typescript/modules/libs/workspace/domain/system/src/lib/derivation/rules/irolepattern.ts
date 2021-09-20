import { RoleType } from "@allors/workspace/meta/system";
import { IPatternBase } from "./IPattern";

export interface IRolePattern extends IPatternBase {
    
    kind: 'RolePattern';
    
    roleType : RoleType;
}