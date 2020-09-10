import { ObjectType } from '@allors/meta/system';

import { Permission } from '../Permission';
import { Composite } from '../Composite';

import { Database } from './Database';

export interface DatabaseObject extends Composite {
  readonly database: Database;
  readonly objectType: ObjectType;
  readonly id: string;
  readonly version: string;
  readonly roleByRoleTypeId: Map<string, any>;

  isPermitted(permission: Permission): boolean;
}
