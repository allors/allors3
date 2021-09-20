import { IWorkspace, IWorkspaceServices, Operations } from '@allors/workspace/domain/system';
import { Class, OperandType } from '@allors/workspace/meta/system';
import { DefaultNumberRanges } from '../collections/ranges/DefaultNumberRanges';
import { Ranges } from '../collections/ranges/Ranges';
import { Configuration } from '../Configuration';
import { DatabaseRecord } from './DatabaseRecord';

export type ServicesBuilder = () => IWorkspaceServices;

export type IdGenerator = () => number;

export abstract class DatabaseConnection {
  ranges: Ranges<number>;

  constructor(public configuration: Configuration, private idGenerator: IdGenerator) {
    this.ranges = new DefaultNumberRanges();
  }

  abstract createWorkspace(): IWorkspace;

  abstract getRecord(id: number): DatabaseRecord | undefined;

  abstract getPermission(cls: Class, operandType: OperandType, operation: Operations): number | undefined;

  nextId(): number {
    return this.idGenerator();
  }
}
