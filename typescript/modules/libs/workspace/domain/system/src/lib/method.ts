import { MethodType } from '@allors/workspace/meta/system';
import { IObject } from './iobject';

export interface Method {
  object: IObject;

  methodType: MethodType;
}
