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
    for (const id of this.database.ranges.enumerate(value)) {
      if (!this.database.grantById.has(id)) {
        this.missingGrantIds.add(id);
      }
    }

    return value;
  }

  checkForMissingRevocations(value: IRange<number>): IRange<number> {
    for (const id of this.database.ranges.enumerate(value)) {
      if (!this.database.permissions.has(id)) {
        this.missingRevocationIds.add(id);
      }
    }

    return value;
  }
}
