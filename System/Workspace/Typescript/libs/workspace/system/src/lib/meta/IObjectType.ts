import { IMetaObject } from './IMetaObject';

// TODO: IComparable<IObjectType>
export interface IObjectType extends IMetaObject {
  singularName: string;

  pluralName: string;

  isUnit: boolean;

  isComposite: boolean;

  isInterface: boolean;

  isClass: boolean;
}
