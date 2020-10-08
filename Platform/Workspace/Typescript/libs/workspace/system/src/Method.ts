import { MethodType } from '@allors/meta/system';
import { DatabaseObject } from './Working/DatabaseObject';

export class Method {
  constructor(public object: DatabaseObject, public methodType: MethodType) {}
}
