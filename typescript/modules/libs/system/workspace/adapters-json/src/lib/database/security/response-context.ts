import { IRange } from '@allors/system/workspace/adapters';
import { DatabaseConnection } from '../database-connection';

export class ResponseContext {
  constructor(private readonly database: DatabaseConnection) {
    this.missingGrantIds = new Set();
    this.missingRevocationIds = new Set();
  }

  missingGrantIds: Set<number>;

  missingRevocationIds: Set<number>;

  checkForMissingGrants(value: IRange<number>): IRange<number> {
    // Synchronization must refresh permissions even when the grant is cached.
    for (const id of this.database.ranges.enumerate(value)) {
      this.missingGrantIds.add(id);
    }

    return value;
  }

  checkForMissingRevocations(value: IRange<number>): IRange<number> {
    // A cached revocation can gain or lose denied permissions without changing id.
    for (const id of this.database.ranges.enumerate(value)) {
      this.missingRevocationIds.add(id);
    }

    return value;
  }
}
