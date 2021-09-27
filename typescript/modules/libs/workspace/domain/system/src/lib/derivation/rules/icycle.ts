import { IChangeSet } from "../../ichangeSet";
import { ISession } from "../../isession";
import { IValidation } from "../ivalidation";

export interface ICycle {
    
    session : ISession;
    
    changeSet : IChangeSet;
    
    validation : IValidation;
}