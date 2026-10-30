import {
  Configuration,
  IWorkspace,
  Operations,
} from '@allors/system/workspace/domain';
import { Class, OperandType } from '@allors/system/workspace/meta';

import { DefaultNumberRanges } from '../collections/ranges/default-number-ranges';
import { Ranges } from '../collections/ranges/ranges';

export type IdGenerator = () => number;

export abstract class DatabaseConnection {
  ranges: Ranges<number>;

  constructor(public configuration: Configuration) {
    this.ranges = new DefaultNumberRanges();
  }

  abstract createWorkspace(): IWorkspace;

  abstract getPermission(
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): number | undefined;

  nextId(): number {
    return this.configuration.idGenerator();
  }
}
