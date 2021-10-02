import { IWorkspace, Operations } from '@allors/workspace/domain/system';
import { Class, OperandType } from '@allors/workspace/meta/system';

import { DefaultNumberRanges } from '../collections/ranges/default-number-ranges';
import { Ranges } from '../collections/ranges/ranges';
import { Configuration } from '../configuration';
import { DatabaseRecord } from './database-record';

export type IdGenerator = () => number;

export abstract class DatabaseConnection {
  ranges: Ranges<number>;

  constructor(public configuration: Configuration) {
    this.ranges = new DefaultNumberRanges();
  }

  abstract createWorkspace(): IWorkspace;

  abstract getRecord(id: number): DatabaseRecord | undefined;

  abstract getPermission(cls: Class, operandType: OperandType, operation: Operations): number | undefined;

  nextId(): number {
    return this.configuration.idGenerator();
  }
}
