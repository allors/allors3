import {
  checkReadInteger,
  readRecordIdentity,
  ReadSecurityKey,
  ReadSecurityKind,
  ReadStoredPermission,
  ReadStoredRecord,
  ReadStoredSecurity,
  ReadStoreScope,
} from './read-store-values';

export interface ReadStoreContents {
  readonly records?: readonly ReadStoredRecord[];
  readonly security?: readonly ReadStoredSecurity[];
  readonly permissions?: readonly ReadStoredPermission[];
}

export interface ReadStorePublication extends ReadStoreContents {
  readonly unavailable?: readonly number[];
}

/**
 * Connection-owned immutable contents and current indexes.
 * Publication receives an already assembled closure, never a raw protocol response.
 */
export class ReadStore {
  readonly scope: ReadStoreScope;
  #records = new Map<string, ReadStoredRecord>();
  #security = new Map<string, ReadStoredSecurity>();
  #permissions = new Map<number, ReadStoredPermission>();
  #currentRecords = new Map<number, ReadStoredRecord>();
  #currentSecurity = new Map<string, ReadStoredSecurity>();
  #currentPermissions = new Map<number, ReadStoredPermission>();
  #unavailable = new Set<number>();
  #recent: ReadStoreContents = Object.freeze({});

  constructor(scope: ReadStoreScope) {
    this.scope = new ReadStoreScope(
      scope.population,
      scope.userId,
      scope.workspace,
      scope.revision,
      scope.context
    );
    Object.freeze(this);
  }

  get recent(): ReadStoreContents {
    return this.#recent;
  }

  retainRecord(candidate: ReadStoredRecord): ReadStoredRecord {
    this.requireScope(candidate.scope);
    const captured = new ReadStoredRecord(
      candidate.scope,
      candidate.id,
      candidate.classTag,
      candidate.dataVersion,
      candidate.grants,
      candidate.revocations,
      candidate.roles
    );
    this.requireScope(captured.scope);
    const previous = this.#records.get(captured.identity);
    if (previous && previous.payloadIdentity !== captured.payloadIdentity) {
      throw new Error(
        `Conflicting record payload for ID ${candidate.id}. Discard the read and obtain a consistent full payload.`
      );
    }
    if (previous) return previous;
    this.#records.set(captured.identity, captured);
    return captured;
  }

  retainSecurity(candidate: ReadStoredSecurity): ReadStoredSecurity {
    this.requireScope(candidate.scope);
    const captured = new ReadStoredSecurity(
      candidate.scope,
      candidate.key,
      candidate.permissionIds
    );
    this.requireScope(captured.scope);
    const previous = this.#security.get(captured.key.identity);
    if (
      previous &&
      JSON.stringify(previous.permissionIds) !==
        JSON.stringify(captured.permissionIds)
    ) {
      throw new Error(
        `Conflicting security body for ${candidate.key.identity}. Discard the read and obtain a consistent full body.`
      );
    }
    if (previous) return previous;
    this.#security.set(captured.key.identity, captured);
    return captured;
  }

  retainPermission(candidate: ReadStoredPermission): ReadStoredPermission {
    this.requireScope(candidate.scope);
    const captured = new ReadStoredPermission(
      candidate.scope,
      candidate.id,
      candidate.classTag,
      candidate.operation,
      candidate.operandTag
    );
    this.requireScope(captured.scope);
    const previous = this.#permissions.get(captured.id);
    if (
      previous &&
      (previous.classTag !== captured.classTag ||
        previous.operation !== captured.operation ||
        previous.operandTag !== captured.operandTag)
    ) {
      throw new Error(
        `Conflicting permission descriptor for ID ${candidate.id}. Refresh the representation scope before accepting a changed encoding.`
      );
    }
    if (previous) return previous;
    this.#permissions.set(captured.id, captured);
    return captured;
  }

  /** Exact historical lookup; no latest-by-ID fallback or implicit security possession. */
  getRecord(candidate: ReadStoredRecord): ReadStoredRecord | undefined;
  getRecord(
    id: number,
    classTag: string,
    dataVersion: number,
    grants: readonly ReadSecurityKey[],
    revocations: readonly ReadSecurityKey[]
  ): ReadStoredRecord | undefined;
  getRecord(
    candidate: ReadStoredRecord | number,
    classTag?: string,
    dataVersion?: number,
    grants?: readonly ReadSecurityKey[],
    revocations?: readonly ReadSecurityKey[]
  ): ReadStoredRecord | undefined {
    if (typeof candidate === 'number') {
      return this.#records.get(
        readRecordIdentity(
          this.scope,
          candidate,
          classTag,
          dataVersion,
          grants,
          revocations
        )
      );
    }
    this.requireScope(candidate.scope);
    return this.#records.get(candidate.identity);
  }

  getSecurity(key: ReadSecurityKey): ReadStoredSecurity | undefined {
    return this.#security.get(key.identity);
  }

  getPermission(id: number): ReadStoredPermission | undefined {
    checkReadInteger(id, 'permission ID');
    return this.#permissions.get(id);
  }

  /** Current record also owns current per-object grant/revocation membership. */
  getCurrentRecord(id: number): ReadStoredRecord | undefined {
    checkReadInteger(id, 'record ID');
    return this.#currentRecords.get(id);
  }

  getCurrentSecurity(
    kind: ReadSecurityKind,
    id: number
  ): ReadStoredSecurity | undefined {
    const key = new ReadSecurityKey(kind, id, 0);
    return this.#currentSecurity.get(this.currentSecurityIdentity(key));
  }

  getCurrentPermission(id: number): ReadStoredPermission | undefined {
    checkReadInteger(id, 'permission ID');
    return this.#currentPermissions.get(id);
  }

  isUnavailable(id: number): boolean {
    checkReadInteger(id, 'record ID');
    return this.#unavailable.has(id);
  }

  /** Validate exact supplied closure first, then advance all current indexes together. */
  publish(publication: ReadStorePublication): Required<ReadStoreContents> {
    const contents = this.capture(publication);
    const unavailable = [...new Set(publication.unavailable ?? [])];
    this.validateOwned(contents);
    unavailable.forEach((id) => checkReadInteger(id, 'unavailable record ID'));
    const records = new Map<number, ReadStoredRecord>();
    const security = new Map<string, ReadStoredSecurity>();
    const currentSecurity = new Map<string, ReadStoredSecurity>();
    const permissions = new Map<number, ReadStoredPermission>();
    for (const record of contents.records) {
      if (records.has(record.id))
        throw new Error(
          `Conflicting current records for ID ${record.id}. Publish one candidate per object.`
        );
      if (unavailable.includes(record.id))
        throw new Error(
          `Record ${record.id} is both available and unavailable. Correct the publication.`
        );
      records.set(record.id, record);
    }
    for (const body of contents.security) {
      const currentIdentity = this.currentSecurityIdentity(body.key);
      if (currentSecurity.has(currentIdentity))
        throw new Error(
          `Conflicting current security versions for ${currentIdentity}. Publish one version per security ID.`
        );
      security.set(body.key.identity, body);
      currentSecurity.set(currentIdentity, body);
    }
    for (const descriptor of contents.permissions) {
      if (permissions.has(descriptor.id))
        throw new Error(
          `Duplicate permission descriptor ${descriptor.id}. Publish each permission once.`
        );
      permissions.set(descriptor.id, descriptor);
    }
    const requiredSecurity = new Set<string>();
    for (const record of records.values()) {
      for (const key of [...record.grants, ...record.revocations]) {
        requiredSecurity.add(key.identity);
        if (!security.has(key.identity))
          throw new Error(
            `Missing security body ${key.identity}. Supply the exact complete security closure before publication.`
          );
      }
    }
    if (requiredSecurity.size !== security.size)
      throw new Error(
        'Unreferenced security body. Publish only the exact record security closure.'
      );
    const requiredPermissions = new Set<number>();
    for (const body of security.values()) {
      for (const id of body.permissionIds) {
        requiredPermissions.add(id);
        if (!permissions.has(id))
          throw new Error(
            `Missing permission descriptor ${id}. Supply the complete permission closure before publication.`
          );
      }
    }
    if (requiredPermissions.size !== permissions.size)
      throw new Error(
        'Unreferenced permission descriptor. Publish only the exact security permission closure.'
      );

    // Build the next indexes without exposing intermediate state. No callbacks or awaits run here.
    const nextRecords = new Map(this.#currentRecords);
    const nextSecurity = new Map(this.#currentSecurity);
    const nextPermissions = new Map<number, ReadStoredPermission>();
    const nextUnavailable = new Set(this.#unavailable);
    for (const record of records.values()) {
      nextRecords.set(record.id, record);
      nextUnavailable.delete(record.id);
    }
    for (const id of unavailable) {
      nextRecords.delete(id);
      nextUnavailable.add(id);
    }
    for (const [key, body] of currentSecurity) nextSecurity.set(key, body);
    // Outside-read records retain membership, while shared security resolves to current bodies.
    for (const body of nextSecurity.values()) {
      for (const id of body.permissionIds) {
        const descriptor =
          permissions.get(id) ?? this.#currentPermissions.get(id);
        if (!descriptor)
          throw new Error(
            `Missing current permission descriptor ${id}. Supply the complete closure before publication.`
          );
        nextPermissions.set(id, descriptor);
      }
    }
    const snapshot = Object.freeze({
      records: Object.freeze(contents.records),
      security: Object.freeze(contents.security),
      permissions: Object.freeze(contents.permissions),
    });
    this.#currentRecords = nextRecords;
    this.#currentSecurity = nextSecurity;
    this.#currentPermissions = nextPermissions;
    this.#unavailable = nextUnavailable;
    this.#recent = snapshot;
    return snapshot;
  }

  /** Called by the connection after dispatched work and Session cleanup have drained. */
  clear(): void {
    this.#records.clear();
    this.#security.clear();
    this.#permissions.clear();
    this.#currentRecords.clear();
    this.#currentSecurity.clear();
    this.#currentPermissions.clear();
    this.#unavailable.clear();
    this.#recent = Object.freeze({});
  }

  private capture(contents: ReadStoreContents): Required<ReadStoreContents> {
    return {
      records: [...(contents.records ?? [])],
      security: [...(contents.security ?? [])],
      permissions: [...(contents.permissions ?? [])],
    };
  }

  private validateOwned(captured: Required<ReadStoreContents>): void {
    for (const record of captured.records) {
      if (this.#records.get(record.identity) !== record)
        throw new Error(
          `Record ${record.id} is not retained by this store. Retain the exact owned candidate before use.`
        );
    }
    for (const body of captured.security) {
      if (this.#security.get(body.key.identity) !== body)
        throw new Error(
          `Security body ${body.key.identity} is not retained by this store. Retain the exact owned body before use.`
        );
    }
    for (const descriptor of captured.permissions) {
      if (this.#permissions.get(descriptor.id) !== descriptor)
        throw new Error(
          `Permission ${descriptor.id} is not retained by this store. Retain the exact owned descriptor before use.`
        );
    }
  }

  private currentSecurityIdentity(key: ReadSecurityKey): string {
    return JSON.stringify([key.kind, key.id]);
  }

  private requireScope(scope: ReadStoreScope): void {
    if (scope.identity !== this.scope.identity)
      throw new Error(
        'Read scope mismatch. Start a new store for the new population, identity, workspace, representation or context.'
      );
  }
}
