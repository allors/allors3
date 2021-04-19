import { IAssociationType } from './IAssociationType';
import { IMetaObject } from './IMetaObject';
import { IRoleType } from './IRoleType';

export interface IRelationType extends IMetaObject
{
    AssociationType: IAssociationType;

    RoleType: IRoleType;

    Multiplicity: Multiplicity;

    IsDerived: boolean;

    IsSynced: boolean;
}
