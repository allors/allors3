import { MethodType } from '@allors/meta/system';
import { SessionObject } from '@allors/workspace/system';

export class Method {
  constructor(public object: SessionObject, public methodType: MethodType) {}
}
