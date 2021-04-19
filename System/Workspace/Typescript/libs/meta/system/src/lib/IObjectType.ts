import { IMetaObject } from './IMetaObject';

// TODO: IComparable<IObjectType>
export interface IObjectType extends IMetaObject {
  IsUnit: boolean;

  IsComposite: boolean;

  IsInterface: boolean;

  IsClass: boolean;

  SingularName: string;

  PluralName: string;

  Type: {};
}
