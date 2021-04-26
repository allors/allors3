import { ObjectType, RoleType } from '@allors/workspace/system';
import { Permission } from './Permission';

export class WritePermission implements Permission {
  constructor(public id: string, public objectType: ObjectType, public roleType: RoleType) {}
}
