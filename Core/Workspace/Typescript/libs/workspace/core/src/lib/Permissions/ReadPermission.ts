import { ObjectType, RoleType } from '@allors/workspace/system';
import { Permission } from './Permission';

export class ReadPermission implements Permission {
  constructor(
    public id: string,
    public objectType: ObjectType,
    public roleType: RoleType,
  ) {}
}
