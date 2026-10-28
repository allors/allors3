import {
  MapMap,
  WorkspaceInitialVersion,
} from '@allors/system/workspace/adapters';
import {
  AccessResponseGrant,
  PermissionResponsePermission,
  PullResponse,
  SyncRequest,
  SyncResponse,
} from '@allors/system/common/protocol-json';
import { Operations } from '@allors/system/workspace/domain';
import {
  Class,
  MethodType,
  OperandType,
  RelationType,
} from '@allors/system/workspace/meta';
import { DatabaseConnection } from '../database-connection';
import { DatabaseRecord } from '../database-record';

export interface PermissionDefinition {
  readonly id: number;
  readonly cls: Class;
  readonly operandType: OperandType;
  readonly operation: Operations;
}

export interface AccessEntry {
  readonly key: string;
  readonly id: number;
  readonly version: number;
  readonly permissionIds: readonly number[];
  readonly definitions: readonly PermissionDefinition[];
}

export class ResponseContext {
  readonly records = new Map<number, DatabaseRecord>();
  readonly synchronizedRecords: DatabaseRecord[] = [];
  readonly entries = new Map<string, AccessEntry>();
  readonly definitions = new Map<number, PermissionDefinition>();
  readonly request: SyncRequest;
  private readonly known = new Map<string, AccessEntry>();
  private readonly pool = new Set<number>();

  constructor(
    private readonly database: DatabaseConnection,
    private readonly response: PullResponse,
    context: string
  ) {
    const grants = this.versions(response.g);
    const revocations = this.versions(response.r);
    const missing: number[] = [];
    const knownGrants: number[][] = [];
    const knownRevocations: number[][] = [];
    for (const object of ResponseContext.array(
      response.p ?? [],
      'Pull objects'
    )) {
      ResponseContext.id(object.i);
      ResponseContext.objectVersion(object.v);
      if (this.pool.has(object.i)) {
        throw new Error('Pull contains duplicate object IDs.');
      }
      this.pool.add(object.i);
      const g = this.bindVersions(object.g, grants);
      const r = this.bindVersions(object.r, revocations);
      const key = ResponseContext.payloadKey(object.i, object.v, g, r);
      const record = database.getPayload(key);
      if (record) {
        this.records.set(object.i, record);
      } else {
        missing.push(object.i);
      }
      for (const [kind, bindings, hints] of [
        ['g', g, knownGrants],
        ['r', r, knownRevocations],
      ] as const) {
        for (const [id, version] of bindings) {
          const accessKey = ResponseContext.accessKey(kind, id, version);
          const entry = database.getAccess(accessKey);
          if (entry && !this.known.has(accessKey)) {
            this.known.set(accessKey, entry);
            hints.push([id, version]);
          }
        }
      }
    }
    this.request = {
      x: context,
      o: missing,
      g: knownGrants,
      r: knownRevocations,
    };
  }

  load(response: SyncResponse): void {
    if (!response) {
      throw new Error('Sync response is missing.');
    }
    const permissionIdsByOperation = new Map<
      Operations,
      MapMap<Class, OperandType, number>
    >();
    for (const wire of ResponseContext.array(
      response.p,
      'Sync permission definitions'
    )) {
      const definition = this.decodeDefinition(wire);
      if (this.definitions.has(definition.id)) {
        throw new Error('Sync contains duplicate permission definitions.');
      }
      this.validateDefinition(definition);
      let permissionIds = permissionIdsByOperation.get(definition.operation);
      if (!permissionIds) {
        permissionIds = new MapMap<Class, OperandType, number>();
        permissionIdsByOperation.set(definition.operation, permissionIds);
      }
      if (permissionIds.get(definition.cls, definition.operandType) != null) {
        throw new Error('Sync contains conflicting permission identities.');
      }
      permissionIds.set(definition.cls, definition.operandType, definition.id);
      this.definitions.set(definition.id, definition);
    }
    const grants = this.readEntries('g', response.g);
    const revocations = this.readEntries('r', response.r);
    const requested = new Set(this.request.o);
    const loaded = new Set<number>();
    for (const object of ResponseContext.array(response.o, 'Sync objects')) {
      ResponseContext.id(object.i);
      ResponseContext.objectVersion(object.v);
      if (!requested.has(object.i) || loaded.has(object.i)) {
        throw new Error('Sync contains an unexpected or duplicate object.');
      }
      loaded.add(object.i);
      const g = ResponseContext.ids(object.g).map((id) => {
        const entry = grants.get(id);
        if (!entry) {
          throw new Error('Sync object references a missing grant.');
        }
        return entry;
      });
      const r = ResponseContext.ids(object.r).map((id) => {
        const entry = revocations.get(id);
        if (!entry) {
          throw new Error('Sync object references a missing revocation.');
        }
        return entry;
      });
      const record = DatabaseRecord.fromResponse(this.database, object, g, r);
      const existing = this.database.getRecord(record.id);
      if (existing && existing.cls !== record.cls) {
        throw new Error(
          `Sync changed the class of object ${record.id} from '${existing.cls.tag}' to '${record.cls.tag}'.`
        );
      }
      this.records.set(record.id, record);
      this.synchronizedRecords.push(record);
    }
    if (loaded.size !== requested.size) {
      throw new Error('Sync omitted a requested object.');
    }
  }

  validateResult(): void {
    for (const results of [this.response.c, this.response.o, this.response.v]) {
      if (results == null) {
        continue;
      }
      if (typeof results !== 'object' || Array.isArray(results)) {
        throw new Error('Pull named results must be dictionaries.');
      }
      const names = Object.keys(results).map((name) => name.toUpperCase());
      if (new Set(names).size !== names.length) {
        throw new Error('Pull contains duplicate result names.');
      }
    }
    if (this.records.size !== this.pool.size) {
      throw new Error('Pull did not load every selected object.');
    }
    for (const ids of Object.values(this.response.c ?? {})) {
      for (const id of ResponseContext.array(ids, 'Pull result collection')) {
        ResponseContext.id(id);
        if (!this.records.has(id)) {
          throw new Error('Pull result references an object outside its pool.');
        }
      }
    }
    for (const id of Object.values(this.response.o ?? {})) {
      if (id === 0 || id == null) {
        continue;
      }
      ResponseContext.id(id);
      if (!this.records.has(id)) {
        throw new Error('Pull result references an object outside its pool.');
      }
    }
  }

  validateDefinition(definition: PermissionDefinition): void {
    const cached = this.database.getDefinition(definition.id);
    const id = this.database.getPermission(
      definition.cls,
      definition.operandType,
      definition.operation
    );
    if (
      cached &&
      (cached.cls !== definition.cls ||
        cached.operandType !== definition.operandType ||
        cached.operation !== definition.operation)
    ) {
      throw new Error(
        `Sync changed the definition of permission ${definition.id}.`
      );
    }
    if (id != null && id !== definition.id) {
      throw new Error(
        `Sync permission ${definition.id} conflicts with cached permission ${id} for class '${definition.cls.tag}', operand '${definition.operandType.operandTag}' and operation ${definition.operation}.`
      );
    }
  }

  private readEntries(
    kind: string,
    values: AccessResponseGrant[]
  ): Map<number, AccessEntry> {
    const entries = new Map<number, AccessEntry>();
    for (const wire of ResponseContext.array(values, 'Sync access entries')) {
      ResponseContext.id(wire.i);
      ResponseContext.version(wire.v);
      if (entries.has(wire.i)) {
        throw new Error('Sync contains duplicate access IDs.');
      }
      const key = ResponseContext.accessKey(kind, wire.i, wire.v);
      let entry: AccessEntry;
      if (wire.p == null) {
        entry = this.known.get(key);
        if (!entry) {
          throw new Error(
            'Sync omitted an access entry that was not advertised as complete.'
          );
        }
      } else {
        const ids = ResponseContext.ids(wire.p);
        const definitions = ids.map((id) => {
          const definition = this.definitions.get(id);
          if (!definition) {
            throw new Error(
              'Sync omitted a permission definition for included access contents.'
            );
          }
          return definition;
        });
        const cached = this.database.getAccess(key);
        if (cached && !ResponseContext.sameIds(cached.permissionIds, ids)) {
          throw new Error(
            `Sync changed the contents of ${kind === 'g' ? 'grant' : 'revocation'} ${wire.i} without changing version ${wire.v}.`
          );
        }
        entry = Object.freeze({
          key,
          id: wire.i,
          version: wire.v,
          permissionIds: Object.freeze(ids),
          definitions: Object.freeze(definitions),
        });
      }
      entries.set(entry.id, entry);
      this.entries.set(key, entry);
    }
    return entries;
  }

  private decodeDefinition(
    wire: PermissionResponsePermission
  ): PermissionDefinition {
    ResponseContext.id(wire.i);
    const meta = this.database.configuration.metaPopulation.metaObjectByTag;
    const cls = meta.get(wire.c) as Class;
    const operand = meta.get(wire.t);
    if (cls?.kind !== 'Class') {
      throw new Error('Sync permission references an unknown class.');
    }
    let operandType: OperandType;
    if (wire.o === Operations.Read || wire.o === Operations.Write) {
      operandType = (operand as RelationType)?.roleType;
      if (
        !operandType ||
        !cls.databaseOriginRoleTypes.has(operandType as any)
      ) {
        throw new Error('Sync permission references an unknown role.');
      }
    } else if (wire.o === Operations.Execute) {
      operandType = operand as MethodType;
      if (!operandType || !cls.methodTypes.has(operandType as MethodType)) {
        throw new Error('Sync permission references an unknown method.');
      }
    } else {
      throw new Error('Sync permission has an unsupported operation.');
    }
    return Object.freeze({ id: wire.i, cls, operandType, operation: wire.o });
  }

  private versions(values: number[][]): Map<number, number> {
    const versions = new Map<number, number>();
    for (const pair of ResponseContext.array(
      values ?? [],
      'Pull access versions'
    )) {
      if (!Array.isArray(pair) || pair.length !== 2) {
        throw new Error('Pull access versions must be [id, version] pairs.');
      }
      ResponseContext.id(pair[0]);
      ResponseContext.version(pair[1]);
      if (versions.has(pair[0])) {
        throw new Error('Pull contains duplicate access IDs.');
      }
      versions.set(pair[0], pair[1]);
    }
    return versions;
  }

  private bindVersions(
    ids: number[],
    versions: Map<number, number>
  ): number[][] {
    return ResponseContext.ids(ids).map((id) => {
      if (!versions.has(id)) {
        throw new Error('Pull omitted a referenced access version.');
      }
      return [id, versions.get(id)];
    });
  }

  static payloadKey(
    id: number,
    version: number,
    grants: readonly (readonly number[])[],
    revocations: readonly (readonly number[])[]
  ): string {
    return JSON.stringify([id, version, grants, revocations]);
  }
  static accessKey(kind: string, id: number, version: number): string {
    return `${kind}:${id}:${version}`;
  }
  static id(value: number): void {
    if (!Number.isSafeInteger(value) || value <= 0) {
      throw new Error('Response IDs must be positive safe integers.');
    }
  }
  static objectVersion(value: number): void {
    ResponseContext.version(value);
    if (value <= WorkspaceInitialVersion) {
      throw new Error('Response object versions must be database versions.');
    }
  }
  static version(value: number): void {
    if (!Number.isSafeInteger(value) || value < 0) {
      throw new Error('Response versions must be non-negative safe integers.');
    }
  }
  static array<T>(value: T[], name: string): T[] {
    if (!Array.isArray(value)) {
      throw new Error(`${name} must be an array.`);
    }
    return value;
  }
  static ids(value: number[]): number[] {
    const ids = [...ResponseContext.array(value ?? [], 'Response ID range')];
    for (let i = 0; i < ids.length; ++i) {
      ResponseContext.id(ids[i]);
      if (i > 0 && ids[i] <= ids[i - 1]) {
        throw new Error('Response ID ranges must be sorted and distinct.');
      }
    }
    return ids;
  }
  static sameIds(a: readonly number[], b: readonly number[]): boolean {
    return a.length === b.length && a.every((id, i) => id === b[i]);
  }
}
