import { MethodType } from '@allors/workspace/system';
import { InternalMetaObject } from './InternalMetaObject';
import { InternalOperandType } from './InternalOperandType';

export interface InternalMethodType extends InternalMetaObject, InternalOperandType, MethodType {
}
