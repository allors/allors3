import { IMetaObject } from './IMetaObject';

// TODO: IComparable<IObjectType>
export interface IObjectType extends IMetaObject {
  isUnit: boolean;

  isComposite: boolean;

  isInterface: boolean;

  isClass: boolean;

  singularName: string;

  pluralName: string;

  //type: {};
}
