import { Class, OperandType } from '@allors/system/workspace/meta';
import { Operations } from './operations';

/**
 * A permission as the server defines it: an operation on an operand type of a class.
 */
export class Permission {
  constructor(
    public readonly id: number,
    public readonly cls: Class,
    public readonly operandType: OperandType,
    public readonly operation: Operations
  ) {}
}
