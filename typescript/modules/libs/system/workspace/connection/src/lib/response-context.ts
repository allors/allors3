import { ICache } from './cache/icache';

/**
 * Collects, while a sync response is stored, the grants and revocations the records name
 * that the cache does not hold yet.
 */
export class ResponseContext {
  readonly missingGrantIds = new Set<number>();

  readonly missingRevocationIds = new Set<number>();

  constructor(private readonly cache: ICache) {}

  checkForMissingGrants(value: number[] | null | undefined): number[] | undefined {
    if (value == null) {
      return undefined;
    }

    for (const id of value) {
      if (this.cache.getGrant(id) == null) {
        this.missingGrantIds.add(id);
      }
    }

    return value;
  }

  checkForMissingRevocations(
    value: number[] | null | undefined
  ): number[] | undefined {
    if (value == null) {
      return undefined;
    }

    for (const id of value) {
      if (this.cache.getRevocation(id) == null) {
        this.missingRevocationIds.add(id);
      }
    }

    return value;
  }
}
