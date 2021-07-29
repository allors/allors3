import { IRange, enumerate } from '@allors/workspace/adapters/system';
import { Database } from '../Database';

export class ResponseContext {
  constructor(private readonly database: Database) {
    this.missingAccessControlIds = new Set();
    this.missingPermissionIds = new Set();
  }

  missingAccessControlIds: Set<number>;

  missingPermissionIds: Set<number>;

  checkForMissingAccessControls(value: IRange): IRange {
    for (const id of enumerate(value)) {
      if (!this.database.accessControlById.has(id)) {
        this.missingAccessControlIds.add(id);
      }
    }

    return value;
  }

  checkForMissingPermissions(value: IRange): IRange {
    for (const id of enumerate(value)) {
      if (!this.database.permissions.has(id)) {
        this.missingPermissionIds.add(id);
      }
    }

    return value;
  }
}
