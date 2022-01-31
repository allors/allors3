import { Class } from '@allors/system/workspace/meta';
import { OnCreate } from '@allors/system/workspace/domain';

export interface CreateRequest {
  readonly kind: 'CreateRequest';
  objectType: Class;
  handlers?: OnCreate[];
}
