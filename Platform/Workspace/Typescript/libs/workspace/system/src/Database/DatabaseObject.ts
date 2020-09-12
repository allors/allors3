import { ObjectType } from '@allors/meta/system';

import { Permission } from '../Permission';
import { DomainObject } from '../DomainObject';

import { Database } from './Database';

export interface DatabaseObject extends DomainObject {
  readonly database: Database;
  readonly objectType: ObjectType;
  readonly id: string;
  readonly version: string;
  readonly roleByRoleTypeId: Map<string, any>;

  isPermitted(permission: Permission): boolean;
}
