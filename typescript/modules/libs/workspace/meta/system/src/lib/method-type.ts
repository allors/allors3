import { Composite } from './composite';
import { MetaObject, MetaObjectExtension } from './meta-object';
import { OperandType, OperandTypeExtension } from './operand-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface MethodTypeTypeExtension extends MetaObjectExtension, OperandTypeExtension {}

export interface MethodType extends MetaObject, OperandType {
  readonly kind: 'MethodType';
  objectType: Composite;
}
