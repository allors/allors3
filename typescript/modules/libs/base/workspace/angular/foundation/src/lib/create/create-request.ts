import { Class } from '@allors/system/workspace/meta';
import { CreateRequestArgument } from './create-request-argument';

export interface CreateRequest {
  readonly kind: 'CreateRequest';
  objectType: Class;
  arguments?: CreateRequestArgument[];
}
