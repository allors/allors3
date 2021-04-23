import { IMethodType } from '@allors/workspace/system';
import { IMetaObjectInternals } from './IMetaObjectInternals';
import { IOperandTypeInternals } from './IOperandTypeInternals';

export interface IMethodTypeInternals extends IMetaObjectInternals, IOperandTypeInternals, IMethodType {
}
