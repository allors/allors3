import { ObjectType, MethodType } from '@allors/meta/core';
import { Permission } from '@allors/workspace/core';

export class ExecutePermission implements Permission {
  constructor(public id: string, public objectType: ObjectType, public methodType: MethodType) {}
}
