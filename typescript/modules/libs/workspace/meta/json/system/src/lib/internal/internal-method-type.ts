import { MethodType } from '@allors/workspace/meta/system';
import { InternalMetaObject } from './internal-meta-object';
import { InternalOperandType } from './internal-operand-type';

export interface InternalMethodType extends InternalMetaObject, InternalOperandType, MethodType {}
