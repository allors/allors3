import { MethodType } from '@allors/workspace/system';
import { DatabaseObject } from './Working/DatabaseObject';

export class Method {
  constructor(public object: DatabaseObject, public methodType: MethodType) {}
}
