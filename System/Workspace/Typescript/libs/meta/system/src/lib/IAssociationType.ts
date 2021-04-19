import { IPropertyType } from "./IPropertyType";
import { IRelationType } from './IRelationType';
import { IRoleType } from "./IRoleType";

export interface IAssociationType extends IPropertyType
{
    RelationType: IRelationType;

    RoleType: IRoleType;
}
