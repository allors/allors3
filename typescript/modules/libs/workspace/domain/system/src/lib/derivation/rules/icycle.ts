import { IChangeSet } from "../../IChangeSet";
import { ISession } from "../../ISession";
import { IValidation } from "../IValidation";

export interface ICycle {
    
    session : ISession;
    
    changeSet : IChangeSet;
    
    validation : IValidation;
}