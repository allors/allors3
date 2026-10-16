import { DatabaseRecord } from '@allors/system/workspace/adapters';
import { Class, RoleType } from '@allors/system/workspace/meta';
import { ReadStoredRecord } from './read-store-values';

/** Exact accepted data, independent of the connection's later current record index. */
export class ScopedDatabaseRecord extends DatabaseRecord {
  constructor(
    cls: Class,
    readonly stored: ReadStoredRecord,
    private readonly roleTags: ReadonlyMap<RoleType, string>
  ) {
    super(cls, stored.id, stored.dataVersion);
  }

  getRole(roleType: RoleType): unknown {
    return this.stored.roles[this.roleTags.get(roleType)];
  }

  isPermitted(): boolean {
    throw new Error(
      'Historical records do not grant authority. Evaluate scoped Session permissions.'
    );
  }
}
