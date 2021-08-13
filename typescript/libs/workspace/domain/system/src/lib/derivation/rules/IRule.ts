import { IObject } from "../../IObject";
import { ICycle } from "./ICycle";
import { IPattern } from "./IPattern";

export interface IRule {
    
    id : string;
    
    patterns : IPattern[];
    
    derive(cycle: ICycle, matches: IObject[]);
}