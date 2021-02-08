import { ObjectType } from '@allors/meta/core';
import { Permission } from '../Permissions/Permission';
import { Database } from './Remote';

export type Unit = string | Date | boolean | number;
export type Composite = string;
export type Composites = string[];
export type Role = Unit | Composite | Composites;

export interface Record {
  readonly database: Database;
  readonly objectType: ObjectType;
  readonly id: string;
  readonly version: string;
  readonly roleByRoleTypeId: Map<string, Role>;

  isPermitted(permission: Permission): boolean;
}
