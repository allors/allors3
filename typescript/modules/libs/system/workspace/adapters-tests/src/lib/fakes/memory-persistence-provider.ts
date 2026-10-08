import {
  AccessResponseGrant,
  AccessResponseRevocation,
  PermissionResponsePermission,
  SyncResponseObject,
} from '@allors/system/common/protocol-json';
import {
  CacheEntries,
  CacheEntryIds,
  CacheKey,
  IPersistenceProvider,
} from '@allors/system/workspace/connection';

/**
 * A persistence provider in memory, for the tests: entries by id per key, and the calls it
 * received. The platform's providers, on IndexedDB for TypeScript, come in a wave of their
 * own.
 */
export class MemoryPersistenceProvider implements IPersistenceProvider {
  loadCount = 0;

  storeCount = 0;

  removeCount = 0;

  clearCount = 0;

  private readonly storeByKey = new Map<string, Store>();

  get keys(): CacheKey[] {
    return [...this.storeByKey.values()].map((v) => v.key);
  }

  objects(key: CacheKey): Map<number, SyncResponseObject> {
    return this.storeFor(key).objects;
  }

  grants(key: CacheKey): Map<number, AccessResponseGrant> {
    return this.storeFor(key).grants;
  }

  revocations(key: CacheKey): Map<number, AccessResponseRevocation> {
    return this.storeFor(key).revocations;
  }

  permissions(key: CacheKey): Map<number, PermissionResponsePermission> {
    return this.storeFor(key).permissions;
  }

  load(key: CacheKey, ids: CacheEntryIds): Promise<CacheEntries> {
    this.loadCount++;

    const store = this.storeFor(key);
    return Promise.resolve({
      objects: pick(store.objects, ids.objects),
      grants: pick(store.grants, ids.grants),
      revocations: pick(store.revocations, ids.revocations),
      permissions: pick(store.permissions, ids.permissions),
    });
  }

  store(key: CacheKey, entries: CacheEntries): Promise<void> {
    this.storeCount++;

    const store = this.storeFor(key);
    for (const object of entries.objects ?? []) {
      store.objects.set(object.i, object);
    }

    for (const grant of entries.grants ?? []) {
      store.grants.set(grant.i, grant);
    }

    for (const revocation of entries.revocations ?? []) {
      store.revocations.set(revocation.i, revocation);
    }

    for (const permission of entries.permissions ?? []) {
      store.permissions.set(permission.i, permission);
    }

    return Promise.resolve();
  }

  remove(key: CacheKey, objectIds: number[]): Promise<void> {
    this.removeCount++;

    const store = this.storeFor(key);
    for (const id of objectIds ?? []) {
      store.objects.delete(id);
    }

    return Promise.resolve();
  }

  clear(key: CacheKey): Promise<void> {
    this.clearCount++;
    this.storeByKey.delete(key.toString());
    return Promise.resolve();
  }

  private storeFor(key: CacheKey): Store {
    let store = this.storeByKey.get(key.toString());
    if (store == null) {
      store = new Store(key);
      this.storeByKey.set(key.toString(), store);
    }

    return store;
  }
}

function pick<T>(byId: Map<number, T>, ids: number[] | undefined): T[] | undefined {
  return ids == null
    ? undefined
    : ids.filter((id) => byId.has(id)).map((id) => byId.get(id));
}

class Store {
  readonly objects = new Map<number, SyncResponseObject>();

  readonly grants = new Map<number, AccessResponseGrant>();

  readonly revocations = new Map<number, AccessResponseRevocation>();

  readonly permissions = new Map<number, PermissionResponsePermission>();

  constructor(public readonly key: CacheKey) {}
}
