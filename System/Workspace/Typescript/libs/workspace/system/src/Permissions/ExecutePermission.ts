import { ObjectType, MethodType } from '@allors/meta/system';
import { Permission } from '@allors/workspace/system';

export class ExecutePermission implements Permission {
  constructor(public id: string, public objectType: ObjectType, public methodType: MethodType) {}
}
