import { Class } from '@allors/system/workspace/meta';
import { OnObjectCreate } from '@allors/system/workspace/domain';

export interface CreateRequest {
  readonly kind: 'CreateRequest';
  objectType: Class;
  handlers?: OnObjectCreate[];
}
