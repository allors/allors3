import { RoleType } from "@allors/workspace/meta/system";
import { IPatternBase } from "./ipattern";

export interface IRolePattern extends IPatternBase {
    
    kind: 'RolePattern';
    
    roleType : RoleType;
}