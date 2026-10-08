import { MethodType } from '@allors/system/workspace/meta';

/**
 * A method to invoke on a database object: the object's id, the version the caller holds,
 * which the server compares with its own, and the method type.
 */
export interface Invocation {
  id: number;

  version: number;

  methodType: MethodType;
}
