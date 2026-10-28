import { PullResponse } from '@allors/system/common/protocol-json';
import {
  DatabaseConnection as SystemDatabaseConnection,
  MapMap,
} from '@allors/system/workspace/adapters';
import {
  Configuration,
  IWorkspace,
  Operations,
} from '@allors/system/workspace/domain';
import { Class, OperandType } from '@allors/system/workspace/meta';
import { DatabaseRecord } from './database-record';
import {
  AccessEntry,
  PermissionDefinition,
  ResponseContext,
} from './security/response-context';
import { Workspace } from '../workspace/workspace';
import { IDatabaseJsonClient } from '../idatabase-json-client';

export class DatabaseConnection extends SystemDatabaseConnection {
  private readonly recordsById = new Map<number, DatabaseRecord>();
  private readonly recordsByKey = new Map<string, DatabaseRecord>();
  private readonly accessByKey = new Map<string, AccessEntry>();
  private readonly permissionsById = new Map<number, PermissionDefinition>();
  private readonly permissionByOperandByClass = new Map<
    Operations,
    MapMap<Class, OperandType, number>
  >();

  constructor(
    configuration: Configuration,
    public client: IDatabaseJsonClient
  ) {
    super(configuration);
    for (const operation of [
      Operations.Read,
      Operations.Write,
      Operations.Execute,
    ]) {
      this.permissionByOperandByClass.set(operation, new MapMap());
    }
  }

  createWorkspace(): IWorkspace {
    return new Workspace(this);
  }

  prepare(response: PullResponse, context: string): ResponseContext {
    return new ResponseContext(this, response, context);
  }

  getAccess(key: string): AccessEntry {
    return this.accessByKey.get(key);
  }
  getDefinition(id: number): PermissionDefinition {
    return this.permissionsById.get(id);
  }
  getPayload(key: string): DatabaseRecord {
    return this.recordsByKey.get(key);
  }

  publish(context: ResponseContext): void {
    // Other sessions may have published while Sync was in flight. Validate again before any write.
    for (const definition of context.definitions.values()) {
      context.validateDefinition(definition);
    }
    for (const entry of context.entries.values()) {
      const existing = this.getAccess(entry.key);
      if (
        existing &&
        !ResponseContext.sameIds(existing.permissionIds, entry.permissionIds)
      ) {
        throw new Error(
          `Sync changed the contents of ${entry.key.startsWith('g:') ? 'grant' : 'revocation'} ${entry.id} without changing version ${entry.version}.`
        );
      }
    }
    for (const record of context.records.values()) {
      const existing = this.getPayload(record.key);
      if (existing && !existing.samePayload(record)) {
        throw new Error(
          `Sync changed the contents of object ${record.id} at data version ${record.version} without changing its security versions.`
        );
      }
      const current = this.getRecord(record.id);
      if (current && current.cls !== record.cls) {
        throw new Error(
          `Sync changed the class of object ${record.id} from '${current.cls.tag}' to '${record.cls.tag}'.`
        );
      }
    }
    for (const definition of context.definitions.values()) {
      this.permissionsById.set(definition.id, definition);
      this.permissionByOperandByClass
        .get(definition.operation)
        .set(definition.cls, definition.operandType, definition.id);
    }
    for (const entry of context.entries.values()) {
      this.accessByKey.set(entry.key, entry);
    }
    for (const record of context.synchronizedRecords) {
      this.recordsByKey.set(record.key, record);
      this.recordsById.set(record.id, record);
    }
  }

  getPermission(
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): number | undefined {
    return this.permissionByOperandByClass
      .get(operation)
      ?.get(cls, operandType);
  }

  getRecord(id: number): DatabaseRecord | undefined {
    return this.recordsById.get(id);
  }
}
