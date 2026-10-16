/** Database IDs and versions use the protocol's exact JavaScript integer range. */
export function checkReadInteger(
  value: number,
  name: string,
  minimum = 1
): void {
  if (!Number.isSafeInteger(value) || value < minimum) {
    throw new Error(
      `Invalid ${name}. Supply a safe integer greater than or equal to ${minimum}.`
    );
  }
}

function checkText(value: string, name: string): void {
  if (typeof value !== 'string' || value.length === 0) {
    throw new Error(`Invalid ${name}. Supply a nonempty string.`);
  }
}

/** A copied, exact scope. Context null and the empty string are different partitions. */
export class ReadStoreScope {
  constructor(
    readonly population: string,
    readonly userId: number,
    readonly workspace: string,
    readonly revision: string,
    readonly context: string | null = null
  ) {
    checkText(population, 'population');
    checkReadInteger(userId, 'user ID');
    checkText(workspace, 'workspace');
    checkText(revision, 'representation revision');
    if (context !== null && typeof context !== 'string') {
      throw new Error(
        'Invalid context. Supply a string or null without normalization.'
      );
    }
    Object.freeze(this);
  }

  get identity(): string {
    return JSON.stringify([
      this.population,
      this.userId,
      this.workspace,
      this.revision,
      this.context,
    ]);
  }
}

function copyScope(scope: ReadStoreScope): ReadStoreScope {
  return new ReadStoreScope(
    scope.population,
    scope.userId,
    scope.workspace,
    scope.revision,
    scope.context
  );
}

export type ReadSecurityKind = 'grant' | 'revocation';

/** Provenance alone does not claim possession of a security body. */
export class ReadSecurityKey {
  constructor(
    readonly kind: ReadSecurityKind,
    readonly id: number,
    readonly version: number
  ) {
    if (kind !== 'grant' && kind !== 'revocation') {
      throw new Error('Invalid security kind. Supply grant or revocation.');
    }
    checkReadInteger(id, 'security ID');
    checkReadInteger(version, 'security version', 0);
    Object.freeze(this);
  }

  get identity(): string {
    return JSON.stringify([this.kind, this.id, this.version]);
  }
}

function provenance(
  keys: readonly ReadSecurityKey[],
  kind: ReadSecurityKind
): readonly ReadSecurityKey[] {
  const byId = new Map<number, ReadSecurityKey>();
  for (const key of keys) {
    const copy = new ReadSecurityKey(key.kind, key.id, key.version);
    if (copy.kind !== kind)
      throw new Error(`Invalid ${kind} provenance. Supply only ${kind} keys.`);
    const existing = byId.get(copy.id);
    if (existing && existing.version !== copy.version) {
      throw new Error(
        `Conflicting ${kind} versions for ID ${copy.id}. Supply one version per record.`
      );
    }
    byId.set(copy.id, copy);
  }
  return Object.freeze([...byId.values()].sort((a, b) => a.id - b.id));
}

/** Metadata-only identity for discovery lookup; does not imply a held role payload. */
export function readRecordIdentity(
  scope: ReadStoreScope,
  id: number,
  classTag: string,
  dataVersion: number,
  grants: readonly ReadSecurityKey[],
  revocations: readonly ReadSecurityKey[]
): string {
  checkReadInteger(id, 'record ID');
  checkText(classTag, 'class tag');
  checkReadInteger(dataVersion, 'data version', 0);
  return JSON.stringify([
    scope.identity,
    id,
    classTag,
    dataVersion,
    provenance(grants, 'grant').map((key) => key.identity),
    provenance(revocations, 'revocation').map((key) => key.identity),
  ]);
}

/**
 * Own supported decoded values, rejecting unknown mutable objects instead of aliasing them.
 * Dictionary/map order is irrelevant; array order and binary view type remain significant.
 */
function copyValue(value: unknown, ancestors = new Set<object>()): unknown {
  if (value === null || typeof value === 'string' || typeof value === 'boolean')
    return value;
  if (typeof value === 'number' && Number.isFinite(value)) return value;
  if (typeof value !== 'object')
    throw new Error(
      'Unsupported role value. Supply finite primitive, dictionary, array, Date or binary values.'
    );
  if (ancestors.has(value))
    throw new Error('Cyclic role value. Supply an acyclic detached payload.');
  if (ancestors.size >= 64)
    throw new Error(
      'Role value nesting exceeds the supported depth of 64. Supply a shallower detached payload.'
    );
  ancestors.add(value);
  try {
    if (value instanceof Date) {
      const time = value.getTime();
      if (!Number.isFinite(time))
        throw new Error('Invalid Date role value. Supply a valid timestamp.');
      return new Date(time);
    }
    if (value instanceof ArrayBuffer) {
      const buffer = new ArrayBuffer(value.byteLength);
      new Uint8Array(buffer).set(new Uint8Array(value));
      return buffer;
    }
    if (ArrayBuffer.isView(value)) {
      const buffer = new ArrayBuffer(value.byteLength);
      new Uint8Array(buffer).set(
        new Uint8Array(value.buffer, value.byteOffset, value.byteLength)
      );
      if (value instanceof DataView) return new DataView(buffer);
      if (value instanceof Uint8Array) return new Uint8Array(buffer);
      if (value instanceof Uint8ClampedArray)
        return new Uint8ClampedArray(buffer);
      if (value instanceof Int8Array) return new Int8Array(buffer);
      if (value instanceof Uint16Array) return new Uint16Array(buffer);
      if (value instanceof Int16Array) return new Int16Array(buffer);
      if (value instanceof Uint32Array) return new Uint32Array(buffer);
      if (value instanceof Int32Array) return new Int32Array(buffer);
      if (value instanceof Float32Array) return new Float32Array(buffer);
      if (value instanceof Float64Array) return new Float64Array(buffer);
      throw new Error(
        'Unsupported binary view. Supply an ArrayBuffer or a supported typed array.'
      );
    }
    if (Array.isArray(value))
      return Array.from(value, (item) => copyValue(item, ancestors));
    if (value instanceof Map) {
      const result = new Map<string, unknown>();
      for (const [key, item] of value) {
        if (typeof key !== 'string')
          throw new Error(
            'Unsupported map key. Supply string dictionary keys.'
          );
        result.set(key, copyValue(item, ancestors));
      }
      return result;
    }
    if (
      Object.getPrototypeOf(value) !== Object.prototype &&
      Object.getPrototypeOf(value) !== null
    ) {
      throw new Error(
        'Unsupported role object. Convert custom instances to detached dictionaries or supported unit values.'
      );
    }
    const result = Object.create(null) as Record<string, unknown>;
    for (const key of Reflect.ownKeys(value)) {
      if (typeof key !== 'string')
        throw new Error('Unsupported dictionary key. Supply string keys.');
      const descriptor = Object.getOwnPropertyDescriptor(value, key);
      if (!descriptor || !('value' in descriptor))
        throw new Error(
          'Unsupported role accessor. Supply plain dictionary values.'
        );
      result[key] = copyValue(descriptor.value, ancestors);
    }
    return result;
  } finally {
    ancestors.delete(value);
  }
}

function valueIdentity(value: unknown): unknown {
  if (value === null || typeof value !== 'object') return value;
  if (value instanceof Date) return ['date', value.getTime()];
  if (value instanceof ArrayBuffer)
    return ['buffer', [...new Uint8Array(value)]];
  if (ArrayBuffer.isView(value))
    return [
      'binary',
      value.constructor.name,
      [...new Uint8Array(value.buffer, value.byteOffset, value.byteLength)],
    ];
  if (Array.isArray(value)) return ['array', value.map(valueIdentity)];
  const entries =
    value instanceof Map ? [...value.entries()] : Object.entries(value);
  return [
    value instanceof Map ? 'map' : 'dictionary',
    entries
      .sort(([left], [right]) => (left < right ? -1 : left > right ? 1 : 0))
      .map(([key, item]) => [key, valueIdentity(item)]),
  ];
}

/** Exact immutable record candidate; each roles access returns detached mutable unit values. */
export class ReadStoredRecord {
  readonly scope: ReadStoreScope;
  readonly grants: readonly ReadSecurityKey[];
  readonly revocations: readonly ReadSecurityKey[];
  readonly identity: string;
  readonly payloadIdentity: string;
  #roles: Readonly<Record<string, unknown>>;

  constructor(
    scope: ReadStoreScope,
    readonly id: number,
    readonly classTag: string,
    readonly dataVersion: number,
    grants: readonly ReadSecurityKey[],
    revocations: readonly ReadSecurityKey[],
    roles: Readonly<Record<string, unknown>>
  ) {
    this.scope = copyScope(scope);
    checkReadInteger(id, 'record ID');
    checkText(classTag, 'class tag');
    checkReadInteger(dataVersion, 'data version', 0);
    this.grants = provenance(grants, 'grant');
    this.revocations = provenance(revocations, 'revocation');
    if (
      !roles ||
      (Object.getPrototypeOf(roles) !== Object.prototype &&
        Object.getPrototypeOf(roles) !== null)
    ) {
      throw new Error(
        'Invalid roles. Supply a dictionary of role tags and decoded values.'
      );
    }
    this.#roles = copyValue(roles) as Readonly<Record<string, unknown>>;
    this.identity = readRecordIdentity(
      this.scope,
      id,
      classTag,
      dataVersion,
      this.grants,
      this.revocations
    );
    this.payloadIdentity = JSON.stringify(valueIdentity(this.#roles));
    Object.freeze(this);
  }

  get roles(): Readonly<Record<string, unknown>> {
    return copyValue(this.#roles) as Readonly<Record<string, unknown>>;
  }
}

/** A separately possessed immutable security body, with canonical permission membership. */
export class ReadStoredSecurity {
  readonly scope: ReadStoreScope;
  readonly key: ReadSecurityKey;
  readonly permissionIds: readonly number[];

  constructor(
    scope: ReadStoreScope,
    key: ReadSecurityKey,
    permissionIds: readonly number[]
  ) {
    this.scope = copyScope(scope);
    this.key = new ReadSecurityKey(key.kind, key.id, key.version);
    const captured = [...permissionIds];
    captured.forEach((id) => checkReadInteger(id, 'permission ID'));
    this.permissionIds = Object.freeze(
      [...new Set(captured)].sort((a, b) => a - b)
    );
    Object.freeze(this);
  }
}

/** Descriptor encoding belongs to the entire store scope, including representation revision. */
export class ReadStoredPermission {
  readonly scope: ReadStoreScope;

  constructor(
    scope: ReadStoreScope,
    readonly id: number,
    readonly classTag: string,
    readonly operation: 1 | 2 | 4 | 8,
    readonly operandTag: string | null = null
  ) {
    this.scope = copyScope(scope);
    checkReadInteger(id, 'permission ID');
    checkText(classTag, 'permission class tag');
    if (![1, 2, 4, 8].includes(operation))
      throw new Error(
        'Invalid permission operation. Supply Create, Read, Write or Execute.'
      );
    if (operation === 1) {
      if (operandTag !== null)
        throw new Error('Invalid create permission. Omit the operand tag.');
    } else {
      checkText(operandTag, 'permission operand tag');
    }
    Object.freeze(this);
  }
}
