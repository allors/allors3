import { Grant } from './grant';
import { Revocation } from './revocation';

/**
 * Collects, while a sync response is stored, the grants and revocations the records name
 * that the connection does not hold yet.
 */
export class ResponseContext {
  readonly missingGrantIds = new Set<number>();

  readonly missingRevocationIds = new Set<number>();

  constructor(
    private readonly grantById: ReadonlyMap<number, Grant>,
    private readonly revocationById: ReadonlyMap<number, Revocation>
  ) {}

  checkForMissingGrants(
    value: number[] | null | undefined
  ): number[] | undefined {
    if (value == null) {
      return undefined;
    }

    for (const id of value) {
      if (!this.grantById.has(id)) {
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
      if (!this.revocationById.has(id)) {
        this.missingRevocationIds.add(id);
      }
    }

    return value;
  }
}
