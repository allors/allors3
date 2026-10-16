import { jsonBytes } from './read-message-size';
import {
  decodeReadRequest,
  decodeReadResponse,
  DiscoverySuccess,
  ReadCache,
  ReadFailure,
  PushReadChanged,
  ReadRequest,
  ReadResponse,
  ReadScope,
  ReadSuccess,
  validateReadRequest,
  validateReadResponse,
} from '@allors/system/common/protocol-json';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { CompletedRead } from './completed-read';
import { captureReadMessage } from './capture-read-message';
import { ReadMetadata } from './read-metadata';
import { ReadFailedError } from './read-failed-error';
import { ReadStore, ReadStoreContents } from './read-store';
import {
  ReadSecurityKey,
  ReadStoredPermission,
  ReadStoredRecord,
  ReadStoredSecurity,
  ReadStoreScope,
} from './read-store-values';

/** Transport/authentication capture belongs to the host; retire before changing credentials. */
export interface ReadTransport {
  read(request: ReadRequest, cancellation?: AbortSignal): Promise<ReadResponse>;
}

export interface ReadCoordinatorConfiguration {
  readonly metaPopulation: MetaPopulation;
  readonly workspace: string;
  readonly revision: string;
  readonly context?: string | null;
  readonly maxHints?: number;
  readonly maxRequestBytes?: number;
  readonly maxResponseBytes?: number;
}

export interface ReadCoordinatorOptions {
  readonly cancellation?: AbortSignal;
}

/** Internal bridge to the connection's cache and admission state; it owns no lifecycle. */
export interface ReadCoordinatorOwner {
  readonly store?: ReadStore;
  bind(store: ReadStore): void;
  assertActive(): void;
  invalidate(): void;
}

interface Hints {
  readonly request: ReadRequest;
  readonly records: readonly ReadStoredRecord[];
  readonly security: readonly (ReadStoredSecurity | undefined)[];
  readonly permissions: readonly ReadStoredPermission[];
}

/** Internal read assembly; the connection owns serialization, admission and cache lifetime. */
export class ReadCoordinator {
  #transport: ReadTransport;
  #metadata: ReadMetadata;
  #workspace: string;
  #revision: string;
  #context: string | null;
  #maxHints: number;
  #maxRequestBytes: number;
  #maxResponseBytes: number;
  #owner: ReadCoordinatorOwner;

  constructor(
    transport: ReadTransport,
    configuration: ReadCoordinatorConfiguration,
    owner: ReadCoordinatorOwner
  ) {
    this.#owner = owner;
    // Configuration and metadata are detached before a request can be scheduled.
    this.#transport = { read: transport.read.bind(transport) };
    this.#workspace = configuration.workspace;
    this.#revision = configuration.revision;
    this.#context = configuration.context ?? null;
    new ReadStoreScope(
      'configuration',
      1,
      this.#workspace,
      this.#revision,
      this.#context
    );
    this.#maxHints = limit(
      configuration.maxHints ?? 256,
      'hint count',
      0,
      70000
    );
    this.#maxRequestBytes = limit(
      configuration.maxRequestBytes ?? 1024 * 1024,
      'request byte limit',
      1,
      1024 * 1024
    );
    this.#maxResponseBytes = limit(
      configuration.maxResponseBytes ?? 16 * 1024 * 1024,
      'response byte limit',
      1,
      16 * 1024 * 1024
    );
    this.#metadata = new ReadMetadata(configuration.metaPopulation);
    Object.freeze(this);
  }

  /** Inputs are captured now, even when an earlier operation still owns the gate. */
  async read(
    request: ReadRequest,
    options: ReadCoordinatorOptions = {}
  ): Promise<CompletedRead> {
    const cancellation = options.cancellation;
    this.check(cancellation);
    const input = captureReadMessage(request);
    validateReadRequest(input);
    const json = JSON.stringify(input);
    this.checkSize(json, this.#maxRequestBytes, 'request');
    const captured = decodeReadRequest(json);
    if (captured.k !== 'read')
      throw new Error(
        'Coordinator requires a read request and performs discovery before completion.'
      );
    if (
      captured.h &&
      (captured.h.r.length || captured.h.s.length || captured.h.p.length)
    )
      throw new Error(
        'The connection owns cache hints. Submit the read selection without caller hints.'
      );
    if ((captured.x ?? null) !== this.#context)
      throw new Error(
        'Read context differs from the configured scope. Create a connection for that exact context.'
      );
    if (captured.sc) this.validateScope(fromScope(captured.sc));
    this.check(cancellation);
    return this.execute(captured, cancellation);
  }

  checkMessageSize(json: string, kind: 'request' | 'response'): void {
    this.checkSize(
      json,
      kind === 'request' ? this.#maxRequestBytes : this.#maxResponseBytes,
      kind
    );
  }

  observeFailure(failure: ReadFailure): void {
    if (
      failure.sc &&
      this.#owner.store &&
      this.#owner.store.scope.identity !== fromScope(failure.sc).identity
    )
      this.#owner.invalidate();
  }

  validateDiscovery(discovery: DiscoverySuccess): void {
    this.check();
    if (
      !this.#owner.store ||
      this.#owner.store.scope.identity !== fromScope(discovery.sc).identity
    )
      throw new Error('Discovery scope differs from the bound connection.');
    for (const record of discovery.p) this.#metadata.requireClass(record.c);
  }

  assertNewIdAvailable(id: number): void {
    if (
      this.#owner.store?.getCurrentRecord(id) ||
      this.#owner.store?.isUnavailable(id)
    )
      throw new Error(
        'Assigned PushRead object ID collides with an existing shared record. Reconcile before further mutation.'
      );
  }

  /** Complete an already discovered query. No discovery request or retained UI-gap request inventory. */
  async complete(
    request: ReadRequest,
    discovery: DiscoverySuccess,
    options: ReadCoordinatorOptions = {}
  ): Promise<CompletedRead> {
    this.check(options.cancellation);
    const captured = decodeReadRequest(
      JSON.stringify(captureReadMessage(request))
    );
    const headers = captureReadMessage(discovery) as DiscoverySuccess;
    validateReadResponse(headers, { ...captured, k: 'discover' });
    for (const record of headers.p) this.#metadata.requireClass(record.c);
    this.check(options.cancellation);
    const store = this.#owner.store;
    if (!store || store.scope.identity !== fromScope(headers.sc).identity)
      throw new Error(
        'Pending read scope is no longer active. Create a new Session.'
      );
    return this.finish(captured, store, headers, options.cancellation);
  }

  /** Admit full affected records through the same metadata and publication checks as Pull. */
  acceptChanged(changed: PushReadChanged): CompletedRead {
    this.check();
    const store = this.#owner.store;
    if (!store || store.scope.identity !== fromScope(changed.sc).identity)
      throw new Error(
        'Changed refresh scope differs from the bound connection.'
      );
    const response: ReadSuccess = {
      i: changed.i,
      sc: changed.sc,
      k: 'complete',
      l: [],
      c: {},
      o: {},
      p: changed.p,
      s: changed.s,
      pm: changed.pm,
    };
    return this.publish(response, undefined, store, changed.u);
  }

  private async execute(
    original: ReadRequest,
    cancellation?: AbortSignal
  ): Promise<CompletedRead> {
    this.check(cancellation);
    const store = this.#owner.store;
    const expected =
      store?.scope ?? (original.sc ? fromScope(original.sc) : undefined);
    if (
      store &&
      original.sc &&
      store.scope.identity !== fromScope(original.sc).identity
    )
      throw new Error(
        'Request scope differs from the coordinator store. Create a new connection for the requested scope.'
      );
    const request: ReadRequest = {
      ...original,
      h: undefined,
      sc: expected ? toScope(expected) : undefined,
    };
    const discovery = await this.send(
      { ...request, k: 'discover' },
      cancellation
    );
    if (discovery.k !== 'discovery')
      throw new Error(
        'Expected discovery headers. Start a new read with a conforming transport.'
      );
    for (const record of discovery.p) this.#metadata.requireClass(record.c);
    request.sc = discovery.sc;

    return this.finish(request, store, discovery, cancellation);
  }

  private async finish(
    request: ReadRequest,
    store: ReadStore | undefined,
    discovery: DiscoverySuccess,
    cancellation?: AbortSignal
  ): Promise<CompletedRead> {
    this.check(cancellation);
    const hints = store ? this.hints(request, store, discovery) : undefined;
    const response = await this.send(hints?.request ?? request, cancellation);
    if (response.k !== 'complete')
      throw new Error(
        'Expected a complete read. Start a new read with a conforming transport.'
      );
    return this.publish(
      response,
      hints,
      store ?? new ReadStore(fromScope(response.sc)),
      response.l
        .filter((entry) => entry.s === 'unavailable')
        .map((entry) => entry.o),
      cancellation
    );
  }

  private publish(
    response: ReadSuccess,
    hints: Hints | undefined,
    target: ReadStore,
    unavailable: number[],
    cancellation?: AbortSignal
  ): CompletedRead {
    const contents = this.assemble(response, hints, target);
    this.preflight(target, contents);
    this.check(cancellation);
    const records = contents.records.map((value) => target.retainRecord(value));
    const security = contents.security.map((value) =>
      target.retainSecurity(value)
    );
    const permissions = contents.permissions.map((value) =>
      target.retainPermission(value)
    );
    const published = target.publish({
      records,
      security,
      permissions,
      unavailable,
    });
    this.#owner.assertActive();
    this.#owner.bind(target);
    const result = new CompletedRead(target.scope, published, response);
    return result;
  }

  private async send(
    request: ReadRequest,
    cancellation?: AbortSignal
  ): Promise<ReadResponse> {
    const json = JSON.stringify(request);
    this.checkSize(json, this.#maxRequestBytes, 'request');
    const captured = decodeReadRequest(json);
    this.check(cancellation);
    // The transport gets another copy and cannot rewrite the request used to validate its answer.
    const received = await this.#transport.read(
      decodeReadRequest(json),
      cancellation
    );
    this.check(cancellation);
    const value = captureReadMessage(received) as ReadResponse;
    const validationRequest = value?.sc
      ? { ...captured, sc: value.sc, x: value.sc.x ?? null }
      : captured;
    validateReadResponse(value, validationRequest);
    const responseJson = JSON.stringify(value);
    this.checkSize(responseJson, this.#maxResponseBytes, 'response');
    const response = decodeReadResponse(responseJson, validationRequest);
    // A demonstrable scope change retires this generation; generic admission failures do not.
    if (response.sc) {
      const scope = fromScope(response.sc);
      try {
        this.validateScope(scope);
        if (captured.sc && scope.identity !== fromScope(captured.sc).identity)
          throw new Error(
            'Read scope changed. Start an explicit new read with a new connection.'
          );
      } catch (error) {
        this.#owner.invalidate();
        if (response.k === 'error') throw new ReadFailedError(response);
        throw error;
      }
    }
    this.check(cancellation);
    if (response.k === 'error') throw new ReadFailedError(response);
    this.validateScope(fromScope(response.sc));
    return response;
  }

  private hints(
    request: ReadRequest,
    store: ReadStore,
    discovery: DiscoverySuccess
  ): Hints {
    const cache: ReadCache = { r: [], s: [], p: [] };
    const records: ReadStoredRecord[] = [];
    const security: (ReadStoredSecurity | undefined)[] = [];
    const permissions: ReadStoredPermission[] = [];
    const securityIndexes = new Map<string, number>();
    const hintedRequest = { ...request, sc: toScope(store.scope), h: cache };
    const fits = () =>
      cache.r.length + cache.s.length + cache.p.length <= this.#maxHints &&
      jsonBytes(JSON.stringify(hintedRequest)) <= this.#maxRequestBytes;
    const addSecurity = (key: ReadSecurityKey) => {
      const known = securityIndexes.get(key.identity);
      if (known !== undefined) return known;
      const index = cache.s.length;
      const body = store.getSecurity(key);
      cache.s.push({
        k: key.kind,
        i: key.id,
        v: key.version,
        h: body !== undefined,
      });
      security.push(body);
      securityIndexes.set(key.identity, index);
      return index;
    };
    const discovered = discovery.p.map((entry) =>
      store.getRecord(
        entry.i,
        entry.c,
        entry.v,
        entry.g.map((index) => securityKey(discovery.s[index])),
        entry.r.map((index) => securityKey(discovery.s[index]))
      )
    );
    const current = request.p.l
      .filter((entry) => entry.o != null)
      .map((entry) => store.getCurrentRecord(entry.o));
    const recordCandidates = [
      ...discovered,
      ...current,
      ...(store.recent.records ?? []),
    ];
    for (const candidate of new Set(recordCandidates)) {
      if (
        !candidate ||
        candidate.scope.identity !== store.scope.identity ||
        store.getRecord(candidate) !== candidate ||
        records.some((record) => record.identity === candidate.identity)
      )
        continue;
      this.#metadata.validateRecord(candidate);
      const previousSecurity = cache.s.length;
      cache.r.push({
        i: candidate.id,
        c: candidate.classTag,
        v: candidate.dataVersion,
        g: candidate.grants.map(addSecurity),
        r: candidate.revocations.map(addSecurity),
      });
      if (fits()) records.push(candidate);
      else {
        cache.r.pop();
        cache.s
          .splice(previousSecurity)
          .forEach((entry) =>
            securityIndexes.delete(securityKey(entry).identity)
          );
        security.splice(previousSecurity);
      }
    }
    const bodyCandidates = [...(store.recent.security ?? [])];
    for (const body of bodyCandidates) {
      if (
        body.scope.identity !== store.scope.identity ||
        store.getSecurity(body.key) !== body ||
        securityIndexes.has(body.key.identity)
      )
        continue;
      addSecurity(body.key);
      if (!fits()) {
        cache.s.pop();
        security.pop();
        securityIndexes.delete(body.key.identity);
      }
    }
    const bodyPermissions: ReadStoredPermission[] = [];
    for (const body of security)
      for (const id of body?.permissionIds ?? [])
        bodyPermissions.push(store.getPermission(id));
    const permissionCandidates = [
      ...bodyPermissions,
      ...(store.recent.permissions ?? []),
    ];
    for (const permission of new Set(permissionCandidates)) {
      if (
        !permission ||
        permission.scope.identity !== store.scope.identity ||
        store.getPermission(permission.id) !== permission ||
        cache.p.includes(permission.id)
      )
        continue;
      this.#metadata.validatePermission(permission);
      cache.p.push(permission.id);
      if (fits()) permissions.push(permission);
      else cache.p.pop();
    }
    return {
      request:
        cache.r.length || cache.s.length || cache.p.length
          ? hintedRequest
          : { ...hintedRequest, h: undefined },
      records: Object.freeze(records),
      security: Object.freeze(security),
      permissions: Object.freeze(permissions),
    };
  }

  private assemble(
    response: ReadSuccess,
    hints: Hints | undefined,
    store: ReadStore
  ): Required<ReadStoreContents> {
    const keys = response.s.map(securityKey);
    const records = response.p.map((entry) =>
      entry.p.r != null
        ? hints.records[entry.p.r]
        : new ReadStoredRecord(
            store.scope,
            entry.i,
            entry.c,
            entry.v,
            entry.g.map((index) => keys[index]),
            entry.r.map((index) => keys[index]),
            this.#metadata.decodeRoles(entry.c, entry.p.ro)
          )
    );
    const security = response.s.map((entry, index) =>
      entry.p.r != null
        ? hints.security[entry.p.r]
        : new ReadStoredSecurity(store.scope, keys[index], entry.p.p)
    );
    const permissions = response.pm.map((entry) =>
      entry.p.r != null
        ? hints.permissions[entry.p.r]
        : new ReadStoredPermission(
            store.scope,
            entry.i,
            entry.p.c,
            entry.p.o,
            entry.p.t ?? null
          )
    );
    if (
      records.some((value) => !value) ||
      security.some((value) => !value) ||
      permissions.some((value) => !value)
    )
      throw new Error(
        'Missing retained reuse body. Start a new read without those hints.'
      );
    const pool = new Map(records.map((record) => [record.id, record]));
    records.forEach((record) => this.#metadata.validateRecord(record, pool));
    permissions.forEach((permission) =>
      this.#metadata.validatePermission(permission)
    );
    const requiredPermissions = new Set<number>();
    for (const body of security)
      for (const id of body.permissionIds) requiredPermissions.add(id);
    if (
      requiredPermissions.size !== permissions.length ||
      permissions.some((permission) => !requiredPermissions.has(permission.id))
    )
      throw new Error(
        'Incomplete or unrelated permission closure. Supply exactly the descriptors required by every resolved security body.'
      );
    const currentSecurity = new Set(
      security.map((body) => JSON.stringify([body.key.kind, body.key.id]))
    );
    if (currentSecurity.size !== security.length)
      throw new Error(
        'Conflicting security versions in one completed read. Supply one current version per security ID.'
      );
    for (const outcome of response.l) {
      if (outcome.s === 'unavailable' && pool.has(outcome.o))
        throw new Error(
          'Unavailable target also appears in the record pool. Correct the read response.'
        );
    }
    return { records, security, permissions };
  }

  private preflight(
    store: ReadStore,
    contents: Required<ReadStoreContents>
  ): void {
    for (const record of contents.records) {
      const current = store.getCurrentRecord(record.id);
      if (current && current.classTag !== record.classTag)
        throw new Error(
          'A refreshed object cannot change its class. Obtain a consistent read before applying its baseline.'
        );
      const previous = store.getRecord(record);
      if (previous && previous.payloadIdentity !== record.payloadIdentity)
        throw new Error(
          `Conflicting record payload for ID ${record.id}. Start a consistent new read.`
        );
    }
    for (const body of contents.security) {
      const previous = store.getSecurity(body.key);
      if (
        previous &&
        JSON.stringify(previous.permissionIds) !==
          JSON.stringify(body.permissionIds)
      )
        throw new Error(
          `Conflicting security body ${body.key.id}. Start a consistent new read.`
        );
    }
    for (const permission of contents.permissions) {
      const previous = store.getPermission(permission.id);
      if (
        previous &&
        (previous.classTag !== permission.classTag ||
          previous.operation !== permission.operation ||
          previous.operandTag !== permission.operandTag)
      )
        throw new Error(
          `Conflicting permission descriptor ${permission.id}. Refresh the representation scope.`
        );
    }
  }

  private validateScope(scope: ReadStoreScope): void {
    if (
      scope.workspace !== this.#workspace ||
      scope.revision !== this.#revision ||
      scope.context !== this.#context
    )
      throw new Error(
        'Read scope workspace, revision or context differs from the configured metadata. Create a new connection with the correct representation.'
      );
  }

  private check(cancellation?: AbortSignal): void {
    const aborted = cancellation?.aborted;
    this.#owner.assertActive();
    if (aborted) {
      const error = new Error(
        'Read cancelled before publication. Start a new read if needed.'
      );
      error.name = 'AbortError';
      throw error;
    }
  }

  private checkSize(json: string, maximum: number, kind: string): void {
    if (jsonBytes(json) > maximum)
      throw new Error(
        `Read ${kind} exceeds its byte limit. Reduce the selection or configure an appropriate limit.`
      );
  }
}

function securityKey(header: {
  k: 'grant' | 'revocation';
  i: number;
  v: number;
}): ReadSecurityKey {
  return new ReadSecurityKey(header.k, header.i, header.v);
}

function fromScope(scope: ReadScope): ReadStoreScope {
  return new ReadStoreScope(
    scope.p,
    scope.u,
    scope.w,
    scope.r,
    scope.x ?? null
  );
}

function toScope(scope: ReadStoreScope): ReadScope {
  return {
    p: scope.population,
    u: scope.userId,
    w: scope.workspace,
    r: scope.revision,
    x: scope.context,
  };
}

function limit(
  value: number,
  name: string,
  minimum: number,
  maximum: number
): number {
  if (!Number.isSafeInteger(value) || value < minimum || value > maximum)
    throw new Error(
      `Invalid ${name}. Supply an integer between ${minimum} and ${maximum}.`
    );
  return value;
}
