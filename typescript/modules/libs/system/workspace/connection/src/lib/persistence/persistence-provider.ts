import {
  AccessResponseGrant,
  AccessResponseRevocation,
  PermissionResponsePermission,
  SyncResponseObject,
} from '@allors/system/common/protocol-json';
import { CacheKey } from '../cache/cache-key';

/**
 * Entries of a user's view in the shape the wire delivers them: what a persistence provider
 * stores and loads. A kind that has no entries is absent.
 */
export interface CacheEntries {
  objects?: SyncResponseObject[];

  grants?: AccessResponseGrant[];

  revocations?: AccessResponseRevocation[];

  permissions?: PermissionResponsePermission[];
}

/**
 * The ids of the entries a connection asks a persistence provider for, by kind. A kind that
 * is not asked for is absent.
 */
export interface CacheEntryIds {
  objects?: number[];

  grants?: number[];

  revocations?: number[];

  permissions?: number[];
}

/**
 * Keeps the user's view beyond the memory cache, under the CacheKey of the user: the
 * objects, grants, revocations and permissions in the shape the wire delivered them, so that
 * restoring them replays the connection's own sync code path. A connection with a provider
 * loads, per pull, the objects the pull advertises that its cache lacks and accepts each only
 * at the version, grant ids and revocation ids the pull advertises; the grants and
 * revocations likewise at their advertised version; what the server then sends is stored
 * before the pull returns. The platform ships providers in a wave of their own.
 */
export interface IPersistenceProvider {
  /**
   * The entries with the given ids that the provider holds under the key; an id it does not
   * hold is left out.
   */
  load(key: CacheKey, ids: CacheEntryIds): Promise<CacheEntries | null | undefined>;

  /**
   * Keeps the entries under the key, each replacing the entry with its id.
   */
  store(key: CacheKey, entries: CacheEntries): Promise<void>;

  /**
   * Forgets the objects with the given ids under the key.
   */
  remove(key: CacheKey, objectIds: number[]): Promise<void>;

  /**
   * Forgets everything under the key; for when the user signs out.
   */
  clear(key: CacheKey): Promise<void>;
}

export function isEmpty(entries: CacheEntries | CacheEntryIds | null | undefined): boolean {
  return (
    entries == null ||
    (!(entries.objects?.length > 0) &&
      !(entries.grants?.length > 0) &&
      !(entries.revocations?.length > 0) &&
      !(entries.permissions?.length > 0))
  );
}
