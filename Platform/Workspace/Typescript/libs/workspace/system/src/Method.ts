import { MethodType } from '@allors/meta/system';
import { DomainObject } from '@allors/workspace/system';

export class Method {
  constructor(public object: DomainObject, public methodType: MethodType) {}
}
