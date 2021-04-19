import { IAssociationType } from "./IAssociationType";
import { IPropertyType } from "./IPropertyType";
import { IRelationType } from "./IRelationType";

export interface IRoleType extends IPropertyType {
  AssociationType: IAssociationType;

  RelationType: IRelationType;

  Size: number;

  Precision: number;

  Scale: number;

  IsRequired: boolean;

  IsUnique: boolean;

  MediaType: string;
}
