import { Class } from '@allors/system/workspace/meta';
import { Initializer } from '@allors/system/workspace/domain';

export interface CreateRequest {
  readonly kind: 'CreateRequest';
  objectType: Class;
  initializer?: Initializer;
}
