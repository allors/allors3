import { MethodType } from '@allors/system/workspace/meta';
import { IObject } from './iobject';

export interface Method {
  object: IObject;

  methodType: MethodType;
}
