import { IMetaObject } from './IMetaObject';

export interface IObjectType extends IMetaObject {
  singularName: string;

  pluralName: string;

  isUnit: boolean;
}
