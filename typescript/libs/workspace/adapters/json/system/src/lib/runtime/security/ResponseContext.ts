import { Numbers } from '@allors/workspace/adapters/system';
import { Database } from '../Database';

export class ResponseContext {
  constructor(private readonly database: Database) {
    this.missingAccessControlIds = new Set();
    this.missingPermissionIds = new Set();
  }

  missingAccessControlIds: Set<number>;

  missingPermissionIds: Set<number>;

  checkForMissingAccessControls(value: Numbers): Numbers {
    return value?.filter((v) => !this.missingAccessControlIds.has(v));
  }

  checkForMissingPermissions(value: Numbers): Numbers {
    return value?.filter((v) => !this.missingPermissionIds.has(v));
  }
}
