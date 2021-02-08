import { ObjectType, RoleType } from '@allors/meta/core';
import { Permission } from './Permission';

export class ReadPermission implements Permission {
  constructor(
    public id: string,
    public objectType: ObjectType,
    public roleType: RoleType,
  ) {}
}
