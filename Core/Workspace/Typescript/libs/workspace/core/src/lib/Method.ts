import { MethodType } from '@allors/meta/core';
import { DatabaseObject } from './Working/DatabaseObject';

export class Method {
  constructor(public object: DatabaseObject, public methodType: MethodType) {}
}
