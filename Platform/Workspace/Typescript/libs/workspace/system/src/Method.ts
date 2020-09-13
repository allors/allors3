import { MethodType } from '@allors/meta/system';
import { DatabaseObject } from './Session/DatabaseObject';

export class Method {
  constructor(public object: DatabaseObject, public methodType: MethodType) {}
}
