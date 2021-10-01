import { MetaObject, MetaObjectExtension } from './meta-object';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface ObjectTypeExtension extends MetaObjectExtension {}

export interface ObjectType extends MetaObject {
  singularName: string;
  pluralName: string;
  isUnit: boolean;
  isComposite: boolean;
  isInterface: boolean;
  isClass: boolean;
}
