import { IWorkspace, IWorkspaceServices, Operations } from '@allors/workspace/domain/system';
import { Class, OperandType } from '@allors/workspace/meta/system';
import { Configuration } from '../Configuration';
import { DatabaseRecord } from './DatabaseRecord';

export type ServicesBuilder = () => IWorkspaceServices;

export type IdGenerator = () => number;

export abstract class Database {
  constructor(public configuration: Configuration, protected servicesBuilder: ServicesBuilder, protected idGenerator: IdGenerator) {}

  abstract createWorkspace(): IWorkspace;

  abstract getRecord(id: number): DatabaseRecord | undefined;

  abstract getPermission(cls: Class, operandType: OperandType, operation: Operations): number | undefined;

  abstract onPushResponse(cls: Class, id: number): DatabaseRecord;
  
  nextId(): number {
    return this.idGenerator();
  }
}
