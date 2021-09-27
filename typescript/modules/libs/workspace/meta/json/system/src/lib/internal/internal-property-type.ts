import { PropertyType } from '@allors/workspace/meta/system';
import { InternalOperandType } from './internal-operand-type';

export interface InternalPropertyType extends InternalOperandType, PropertyType {}
