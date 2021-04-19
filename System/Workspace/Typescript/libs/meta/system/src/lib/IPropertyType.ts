import { IComposite } from "./IComposite";
import { IObjectType } from "./IObjectType";
import { IOperandType } from "./IOperandType";

export interface IPropertyType extends IOperandType // TODO: IComparable
    {
        Origin: Origin;

        Name: string;

        SingularName: string;

        PluralName: string;

        ObjectType: IObjectType;

        IsOne: boolean;

        IsMany: boolean;

        Get(strategy: IStrategy, ofType: IComposite): object;
    }
