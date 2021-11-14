import { MetaObject } from './meta-object';

export interface ObjectType extends MetaObject {
  _: unknown;
  singularName: string;
  pluralName: string;
  isUnit: boolean;
  isComposite: boolean;
  isInterface: boolean;
  isClass: boolean;
}
