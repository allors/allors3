import { ObjectType } from '@allors/meta/system';

import { Database } from './Database';
import { Permission } from './Permission';

export interface DatabaseObject {
  readonly workspace: Database;
  readonly objectType: ObjectType;
  readonly id: string;
  readonly version: string;
  readonly roleByRoleTypeId: Map<string, any>;

  isPermitted(permission: Permission): boolean;
}
